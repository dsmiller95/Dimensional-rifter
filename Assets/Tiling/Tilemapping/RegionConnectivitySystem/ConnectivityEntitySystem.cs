using Assets.WorldObjects;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Wall.DOTS;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    [AlwaysUpdateSystem]
    public class ConnectivityEntitySystem : SystemBase
    {
        private static readonly int TileTypesToImpassibleQueryBatchSize = 128;
        private static readonly float TimeBetweenConnectivityUpdates = 1;

        private NativeDisposableHotSwap<NativeHashMap<UniversalCoordinate, uint>> regionConnectivityClassification;
        private NativeDisposableHotSwap<NativeHashSet<UniversalCoordinate>> blockedCoordinates;

        public bool HasRegionMaps => regionConnectivityClassification.ActiveData.HasValue && blockedCoordinates.ActiveData.HasValue;
        public NativeHashSet<UniversalCoordinate> BlockedCoordinates => blockedCoordinates.ActiveData.Value;
        public NativeHashMap<UniversalCoordinate, uint> Regions => regionConnectivityClassification.ActiveData.Value;

        /// <summary>
        /// an estimate of what fraction of tiles will be blocking; used in memory allocation
        /// </summary>
        private float BlockingTilesRatioEstimate = 0.2f;
        private int lastTotalTiles;

        /// <summary>
        /// Set when the job is scheduled
        ///     set to null when the job is completed and the system has handled the completion
        /// </summary>
        private JobHandle? regionConnectivityDep = null;
        private double lastRegionConnectivityJob = 0;

        protected override void OnCreate()
        {
            lastRegionConnectivityJob = TimeBetweenConnectivityUpdates * -10;
            longRunningDisposables = new List<IDisposable>();
            regionConnectivityClassification = new NativeDisposableHotSwap<NativeHashMap<UniversalCoordinate, uint>>();
            blockedCoordinates = new NativeDisposableHotSwap<NativeHashSet<UniversalCoordinate>>();
        }

        protected override void OnUpdate()
        {
            if (!regionConnectivityDep.HasValue)
            {
                TryScheduleJob();
                return;
            }
            if (regionConnectivityDep.Value.IsCompleted)
            {
                regionConnectivityClassification.HotSwapToPending();
                blockedCoordinates.HotSwapToPending();

                regionConnectivityDep.Value.Complete();
                // update the estimate based on past results
                var totalBlockedPositions = blockedCoordinates.ActiveData?.Count() ?? -1;
                BlockingTilesRatioEstimate = ((float)totalBlockedPositions) / lastTotalTiles;

                DisposeAllWorkingData();
                Debug.Log($"[CONNECTIVITY] Classified {regionConnectivityClassification.ActiveData?.Count() ?? -1} points. {BlockingTilesRatioEstimate * 100:F1}% of tiles are blocking");
                regionConnectivityDep = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (regionConnectivityDep.HasValue)
            {
                regionConnectivityDep.Value.Complete();
                DisposeAllWorkingData();
            }
            regionConnectivityClassification.Dispose();
            blockedCoordinates.Dispose();
        }

        private void TryScheduleJob()
        {
            if (Time.ElapsedTime - lastRegionConnectivityJob > TimeBetweenConnectivityUpdates)
            {
                lastRegionConnectivityJob = Time.ElapsedTime;
                ScheduleNewConnectivityJob();
            }
        }

        private void ScheduleNewConnectivityJob()
        {
            if (CombinationTileMapManager.instance == null)
            {
                // must be in a scene without all the coordinates and ranges
                return;
            }
            var ranges = new NativeArray<UniversalCoordinateRange>(CombinationTileMapManager.instance.allRegions.Select(data => data.baseRange).ToArray(), Allocator.Persistent);
            longRunningDisposables.Add(ranges);

            lastTotalTiles = ranges.Sum(x => x.TotalCoordinateContents());
            var blockingTilesSizeEstimate = (int)(lastTotalTiles * BlockingTilesRatioEstimate * 1.1);
            Profiler.BeginSample("Populate blocking set");
            var pendingBlockedPositionIndexed = new NativeHashSet<UniversalCoordinate>(blockingTilesSizeEstimate, Allocator.Persistent);
            blockedCoordinates.AssignPending(pendingBlockedPositionIndexed);

            // ------data queries------
            var concurrentWriter = pendingBlockedPositionIndexed.AsParallelWriter();
            // only pass through the job which queries the entities
            //  all other jobs will run long
            Dependency = ScheduleEntityBlockingJob(concurrentWriter, Dependency);
            var dataQueryDep = ScheduleTileMapBlockingJob(ranges, concurrentWriter, Dependency);
            Profiler.EndSample();


            // -----region classification-----
            var outputRegionClassification = new NativeHashMap<UniversalCoordinate, uint>(lastTotalTiles, Allocator.Persistent);
            regionConnectivityClassification.AssignPending(outputRegionClassification);

            // keep track of the full dep, don't schedule more until this is complete
            regionConnectivityDep = ScheduleRegionClassificationJobs(ranges, pendingBlockedPositionIndexed, outputRegionClassification, dataQueryDep);
            // TODO: add support here for links between like map ranges, to merge into one range
        }

        private JobHandle ScheduleRegionClassificationJobs(
            NativeArray<UniversalCoordinateRange> ranges,
            NativeHashSet<UniversalCoordinate> impassibleCoordiantes,
            NativeHashMap<UniversalCoordinate, uint> outputRegionMasks,
            JobHandle dependency = default)
        {
            Profiler.BeginSample("Schedule: region classification");
            /** seed regions with all members that have a navigation member
             *  assumptions:
             *    only gameObjects care about reachability
             *    only gameObjects with a NavigationMember move
             *    only gameObjects with a NavigationMember care about reachability
             */
            var allActors = UnityEngine.Object.FindObjectsOfType<TileMapNavigationMember>();
            var seedPoints = new NativeArray<UniversalCoordinate>(
                allActors.Select(x => x.CoordinatePosition).ToArray(),
                Allocator.Persistent);
            longRunningDisposables.Add(seedPoints);
            foreach (var coordinateRange in ranges)
            {
                // inputs have to be persistent, this job is long-running
                var regionClassifierJob = new CoordinateRangeToRegionMapJob
                {
                    coordinateRangeToIterate = coordinateRange,
                    impassableTiles = impassibleCoordiantes,
                    seedPoints = seedPoints,
                    outputRegionBitMasks = outputRegionMasks,
                    workingFringe = new NativeQueue<UniversalCoordinate>(Allocator.Persistent),
                    workingNeighborCoordinatesSwapSpace = new NativeArray<UniversalCoordinate>(UniversalCoordinate.MaxNeighborCount, Allocator.Persistent)
                };

                dependency = regionClassifierJob.Schedule(dependency);
                longRunningDisposables.Add(regionClassifierJob.workingFringe);
            }
            Profiler.EndSample();
            return dependency;
        }

        private JobHandle ScheduleEntityBlockingJob(NativeHashSet<UniversalCoordinate>.ParallelWriter concurrentWriter, JobHandle dependency = default)
        {
            Profiler.BeginSample("Schedule: entity blocking");
            var resultJob = Entities
                .ForEach((int entityInQueryIndex, Entity self, in TileBlockingComponent blocking, in UniversalCoordinatePositionComponent position) =>
                {
                    if (blocking.CurrentlyBlocking)
                    {
                        concurrentWriter.Add(position.Value);
                    }
                }).ScheduleParallel(dependency);
            Profiler.EndSample();
            return resultJob;
        }

        private IList<IDisposable> longRunningDisposables;
        private void DisposeAllWorkingData()
        {
            foreach (var disposable in longRunningDisposables)
            {
                disposable.Dispose();
            }
            longRunningDisposables.Clear();
        }

        private JobHandle ScheduleTileMapBlockingJob(
            NativeArray<UniversalCoordinateRange> ranges,
            NativeHashSet<UniversalCoordinate>.ParallelWriter concurrentWriter,
            JobHandle dependency = default)
        {
            Profiler.BeginSample("Schedule: tile Map blocking");
            var tileTypeDict = CombinationTileMapManager.instance.everyMember.GetTileTypesByCoordinateReadonlyCollection();
            var impassibleIDs = GetImpassibleIDSet(Allocator.TempJob);

            foreach (var range in ranges)
            {
                var nextRangeJob = new SelectFromCoordinateRangeJob<int>
                {
                    range = range,
                    hashMapToFilter = tileTypeDict,
                    ValuesToSelectFor = impassibleIDs,
                    HashSetWriter = concurrentWriter
                };
                var totalCoordinates = range.TotalCoordinateContents();
                dependency = nextRangeJob.Schedule(
                    totalCoordinates,
                    TileTypesToImpassibleQueryBatchSize,
                    dependency);
            }

            dependency = impassibleIDs.Dispose(dependency);
            Profiler.EndSample();
            return dependency;
        }

        private NativeHashSet<int> GetImpassibleIDSet(Allocator allocator)
        {
            var impassibleTileIDs = CombinationTileMapManager.instance.everyMember.GetTileInfoByTypeIndex()
                .Select((info, index) => new
                {
                    info,
                    index
                })
                .Where(data => !data.info.isPassable)
                .Select(data => data.index)
                .ToArray();
            var impassibleIDsSet = new NativeHashSet<int>(impassibleTileIDs.Length, allocator);
            foreach (var impassableID in impassibleTileIDs)
            {
                impassibleIDsSet.Add(impassableID);
            }
            return impassibleIDsSet;
        }
    }
}
