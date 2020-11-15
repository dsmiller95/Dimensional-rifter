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

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{

    [AlwaysUpdateSystem]
    public class ConnectivityEntitySystem : SystemBase
    {
        /// <summary>
        /// an estimate of what fraction of tiles will be blocking; used in memory allocation
        /// </summary>
        private static readonly float BlockingTilesRatioEstimate = 0.2f;
        private static readonly int TileTypesToImpassibleQueryBatchSize = 128;
        private static readonly float TimeBetweenConnectivityUpdates = 1;

        private NativeCollectionHotSwap regionConnectivityClassification;

        private EntityQuery BlockingEntities;

        /// <summary>
        /// Set when the job is scheduled
        ///     set to null when the job is completed and the system has handled the completion
        /// </summary>
        private JobHandle? regionConnectivityDep = null;
        /// <summary>
        /// used to ensure early-completion of the short-running jobs which pull data into the long-running job
        /// </summary>
        private JobHandle? dataQueryDep = null;
        private double lastRegionConnectivityJob = 0;

        protected override void OnCreate()
        {
            longRunningDisposables = new List<IDisposable>();
            regionConnectivityClassification = new NativeCollectionHotSwap();
        }

        protected override void OnUpdate()
        {
            if (!regionConnectivityDep.HasValue)
            {
                TryScheduleJob();
                return;
            }
            
            if(dataQueryDep.HasValue && dataQueryDep.Value.IsCompleted)
            {
                dataQueryDep.Value.Complete();
                dataQueryDep = null;
            }
            if (regionConnectivityDep.Value.IsCompleted)
            {
                regionConnectivityDep.Value.Complete();
                DisposeAllWorkingData();
                regionConnectivityClassification.HotSwapToPending();
                Debug.Log($"Classified {regionConnectivityClassification.ActiveData?.Count() ?? -1} points");
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
        }



        private void TryScheduleJob()
        {
            if (Time.ElapsedTime - lastRegionConnectivityJob > TimeBetweenConnectivityUpdates)
            {
                Debug.Log("Scheduling new job");
                lastRegionConnectivityJob = Time.ElapsedTime;
                ScheduleNewConnectivityJob();
            }
        }

        private void ScheduleNewConnectivityJob()
        {
            var ranges = new NativeArray<UniversalCoordinateRange>(CombinationTileMapManager.instance.allRegions.Select(data => data.baseRange).ToArray(), Allocator.Persistent);

            var totalCoordinates = ranges.Sum(x => x.TotalCoordinateContents());
            var blockingTilesSizeEstimate = (int)(totalCoordinates * BlockingTilesRatioEstimate);

            var blockedPositionsIndexed = new NativeHashSet<UniversalCoordinate>(blockingTilesSizeEstimate, Allocator.Persistent);

            // ------data queries------
            var concurrentWriter = blockedPositionsIndexed.AsParallelWriter();
            var entityBlockingJob = Entities
                .WithStoreEntityQueryInField(ref BlockingEntities)
                .ForEach((int entityInQueryIndex, Entity self, in TileBlockingComponent blocking, in UniversalCoordinatePositionComponent position) =>
                {
                    if (blocking.CurrentlyBlocking)
                    {
                        concurrentWriter.Add(position.Value);
                    }
                }).ScheduleParallel(Dependency);

            // only pass through the job which queries the entities
            //  all other jobs will run long
            Dependency = entityBlockingJob;

            var tileTypes = CombinationTileMapManager.instance.everyMember.GetTileTypesByCoordinateReadonlyCollection().GetKeyValueArrays(Allocator.Persistent);
            var impassibleIDs = GetImpassibleIDSet(Allocator.Persistent);

            var tileTypesToImpassibleJob = new SelectKeysfromHashMapJob<UniversalCoordinate, int>
            {
                hashMapToFilter = tileTypes,
                ValuesToSelectFor = impassibleIDs,
                HashSetWriter = concurrentWriter
            };
            var tileImpassibleJob = tileTypesToImpassibleJob.Schedule(tileTypes.Length, TileTypesToImpassibleQueryBatchSize, entityBlockingJob);
            tileImpassibleJob = JobHandle.CombineDependencies(
                tileTypes.Dispose(tileImpassibleJob),
                impassibleIDs.Dispose(tileImpassibleJob));

            dataQueryDep = tileImpassibleJob;

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

            // -----region classification-----

            var allRegionConnectivityClassifications = tileImpassibleJob;

            var outputRegionClassification = new NativeHashMap<UniversalCoordinate, uint>(totalCoordinates, Allocator.Persistent);
            regionConnectivityClassification.AssignPending(outputRegionClassification);


            foreach (var coordinateRange in ranges)
            {
                // inputs have to be persistent, this job is long-running
                var regionClassifierJob = new CoordinateRangeToRegionMapJob
                {
                    coordinateRangeToIterate = coordinateRange,
                    impassableTiles = blockedPositionsIndexed,
                    seedPoints = seedPoints,
                    outputRegionBitMasks = outputRegionClassification,
                    workingFringe = new NativeQueue<UniversalCoordinate>(Allocator.Persistent),
                    workingNeighborCoordinatesSwapSpace = new NativeArray<UniversalCoordinate>(UniversalCoordinate.MaxNeighborCount, Allocator.Persistent)
                };

                allRegionConnectivityClassifications = regionClassifierJob.Schedule(allRegionConnectivityClassifications);
                longRunningDisposables.Add(regionClassifierJob.workingFringe);
            }
            longRunningDisposables.Add(blockedPositionsIndexed);
            longRunningDisposables.Add(ranges);
            longRunningDisposables.Add(seedPoints);



            // keep track of the full dep, don't schedule more until this is complete
            regionConnectivityDep = allRegionConnectivityClassifications;
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
