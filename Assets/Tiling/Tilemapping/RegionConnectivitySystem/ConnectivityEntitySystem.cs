using Assets.WorldObjects;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Buildings.DOTS.Anchor;
using Assets.WorldObjects.Members.Wall.DOTS;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{

    [AlwaysUpdateSystem]
    public class ConnectivityEntitySystem : SystemBase
    {
        private enum ErrorCodes
        {
            NONE = 0,
            FAIL_ANCHORS_SAME_REGION
        }

        private static readonly int CoordinateRangeJobsBatchSize = 128;
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

        private NativeArray<int> regionCounter;
        private NativeArray<ErrorCodes> errorCode;
        private NativeHashMap<int, int> remaps;
        /// <summary>
        /// Set when the job is scheduled
        ///     set to null when the job is completed and the system has handled the completion
        /// </summary>
        private JobHandle? regionConnectivityDep = null;
        private double lastRegionConnectivityJob = 0;

        private EntityQuery anchorEntities;

        protected override void OnCreate()
        {
            lastRegionConnectivityJob = TimeBetweenConnectivityUpdates * -10;
            longRunningDisposables = new List<IDisposable>();
            regionConnectivityClassification = new NativeDisposableHotSwap<NativeHashMap<UniversalCoordinate, uint>>();
            blockedCoordinates = new NativeDisposableHotSwap<NativeHashSet<UniversalCoordinate>>();

            anchorEntities = GetEntityQuery(
                ComponentType.ReadOnly<TilemapAnchorComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>());
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

                if(errorCode[0] != ErrorCodes.NONE)
                {
                    Debug.LogError($"[CONNECTIVITY] Error during classification: {Enum.GetName(typeof(ErrorCodes), errorCode[0])}");
                }

                Debug.Log($"[CONNECTIVITY] Classified {regionConnectivityClassification.ActiveData?.Count() ?? -1} points. {BlockingTilesRatioEstimate * 100:F1}% of tiles are blocking. {regionCounter[0]} distinct regions classified");

                var remapString = "[CONNECTIVITY] Remaps: ";
                foreach (var regionMapping in remaps)
                {
                    remapString += $"[{regionMapping.Key} : {regionMapping.Value}]";
                }
                Debug.Log(remapString);


                DisposeAllWorkingData();
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
            var ranges = new NativeArray<UniversalCoordinateRange>(
                CombinationTileMapManager.instance.regionBehaviors
                    .Select(data => data.MyOwnData.baseRange).ToArray(),
                Allocator.Persistent);
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
            // TODO: read data about which items in the range are blocked by other ranges
            Dependency = ScheduleEntityBlockingJob(concurrentWriter, Dependency);
            var dataQueryDep = ScheduleTileMapBlockingJob(ranges, concurrentWriter, Dependency);
            Profiler.EndSample();

            Profiler.BeginSample("Sample for seed points");
            var anchorComponents = anchorEntities.ToComponentDataArrayAsync<TilemapAnchorComponent>(Allocator.Persistent, out var anchorComponent_job);
            longRunningDisposables.Add(anchorComponents);
            var anchorPositions = anchorEntities.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.Persistent, out var anchorPosition_job);
            longRunningDisposables.Add(anchorPositions);
            // make sure that the primary Dependency includes all readers of component data
            Dependency = JobHandle.CombineDependencies(Dependency, anchorComponent_job, anchorPosition_job);

            var totalAnchors = anchorEntities.CalculateEntityCount();
            var seedPoints = ScheduleSeedPoints(
                anchorComponents,
                anchorPositions,
                totalAnchors,
                Dependency,
                out var seedPointJob);

            dataQueryDep = JobHandle.CombineDependencies(dataQueryDep, seedPointJob);
            Profiler.EndSample();


            // -----region classification-----

            var regionIndexes = new NativeHashMap<UniversalCoordinate, int>(lastTotalTiles, Allocator.Persistent);
            longRunningDisposables.Add(regionIndexes);

            // keep track of the full dep, don't schedule more until this is complete
            var individualRegionJob = ScheduleRegionClassificationJobs(
                ranges,
                seedPoints,
                pendingBlockedPositionIndexed,
                regionIndexes,
                out var allocatedRegions,
                out var regionCounter,
                dataQueryDep);
            // TODO: add support here for links between like map ranges, to merge into one range
            this.regionCounter = regionCounter;
            var errorCode = new NativeArray<ErrorCodes>(1, Allocator.Persistent);
            longRunningDisposables.Add(errorCode);
            this.errorCode = errorCode;


            var regionRemaps = new NativeHashMap<int, int>(totalAnchors, Allocator.Persistent);
            this.remaps = regionRemaps;
            longRunningDisposables.Add(regionRemaps);
            individualRegionJob = RemapRegions(
                totalAnchors,
                anchorComponents,
                anchorPositions,
                regionIndexes,
                regionRemaps,
                individualRegionJob);

            var outputRegionClassification = new NativeHashMap<UniversalCoordinate, uint>(lastTotalTiles, Allocator.Persistent);
            regionConnectivityClassification.AssignPending(outputRegionClassification);
            individualRegionJob = ScheduleIndexesToBitMaskJobs(
                ranges,
                regionIndexes,
                regionRemaps,
                outputRegionClassification,
                individualRegionJob);

            this.regionConnectivityDep = individualRegionJob;
        }

        private JobHandle RemapRegions(
            int totalAnchors,
            NativeArray<TilemapAnchorComponent> anchorComponentsInput,
            NativeArray<UniversalCoordinatePositionComponent> anchorPositionsInput,
            NativeHashMap<UniversalCoordinate, int> regionIndexesInput,
            NativeHashMap<int, int> regionRemapsOuput,
            JobHandle dependency)
        {
            var errorCode_lambdaCapture = errorCode;
            return Job.WithBurst()
                .WithCode(() =>
                {
                    int maxMappedRegion = 0;
                    for (int anchorIndex = 0; anchorIndex < totalAnchors; anchorIndex++)
                    {
                        var anchoredPos = anchorComponentsInput[anchorIndex].destinationCoordinate;
                        var anchorPos = anchorPositionsInput[anchorIndex].Value;
                        var anchoredRegion = regionIndexesInput[anchoredPos];
                        var anchorRegion = regionIndexesInput[anchorPos];
                        if (anchoredRegion == anchorRegion)
                        {
                            errorCode_lambdaCapture[0] = ErrorCodes.FAIL_ANCHORS_SAME_REGION;
                            return;
                        }
                        var regionA = math.min(anchorRegion, anchoredRegion);
                        var regionB = math.max(anchorRegion, anchoredRegion);
                        // Map region B to A, to get smallest region indexes
                        if (regionRemapsOuput.TryGetValue(regionB, out var existingMapping))
                        {
                            if (existingMapping == regionA)
                            {
                                continue;
                            }
                            // If C -> B, and then C -> A, then B -> A and C -> A
                            var newRemapRegion = math.min(existingMapping, regionA);
                            var extraMapping = math.max(existingMapping, regionA);
                            regionRemapsOuput[regionB] = newRemapRegion;
                            regionRemapsOuput[extraMapping] = newRemapRegion;
                            continue;
                        }
                        regionRemapsOuput[regionB] = regionA;
                        maxMappedRegion = math.max(regionB, maxMappedRegion);
                    }
                    for (int i = 0; i < maxMappedRegion; i++)
                    {
                        // if C -> B, and then B -> A, then C -> A
                        // only requires one iteration because all mappings point to a smaller index, which guarantees all mappings less than the current index have no chains
                        if (regionRemapsOuput.TryGetValue(i, out var mappedToRegion))
                        {
                            if (regionRemapsOuput.TryGetValue(mappedToRegion, out var transitiveMappedRegion))
                            {
                                regionRemapsOuput[i] = transitiveMappedRegion;
                            }
                        }
                    }
                }).Schedule(dependency);
        }

        private JobHandle ScheduleIndexesToBitMaskJobs(
            NativeArray<UniversalCoordinateRange> ranges,
            NativeHashMap<UniversalCoordinate, int> inputRegionIndexes,
            NativeHashMap<int, int> inputRegionRemaps,
            NativeHashMap<UniversalCoordinate, uint> outputRegionBitMasks,
            JobHandle dependency)
        {
            var outputWriter = outputRegionBitMasks.AsParallelWriter();
            foreach (var range in ranges)
            {
                var nextRangeJob = new MapIndexesToBitMaskRegionsJob
                {
                    range = range,
                    regionIndexes_input = inputRegionIndexes,
                    regionRemappings_input = inputRegionRemaps,
                    regionBitMasks_output = outputWriter
                };
                var totalCoordinates = range.TotalCoordinateContents();
                dependency = nextRangeJob.Schedule(
                    totalCoordinates,
                    CoordinateRangeJobsBatchSize,
                    dependency);
            }
            return dependency;
        }

        private NativeArray<UniversalCoordinate> ScheduleSeedPoints(
            NativeArray<TilemapAnchorComponent> anchors,
            NativeArray<UniversalCoordinatePositionComponent> anchorPositions,
            int calculatedAnchorEntityLength,
            JobHandle dependency,
            out JobHandle newJob)
        {
            /** seed regions with all members that have a navigation member
             *  assumptions:
             *    only gameObjects care about reachability
             *    only gameObjects with a NavigationMember move
             *    only gameObjects with a NavigationMember care about reachability
             */
            var actorSeeds = new NativeArray<UniversalCoordinate>(GetActorPositions(), Allocator.TempJob);
            var seedPoints = new NativeArray<UniversalCoordinate>(actorSeeds.Length + calculatedAnchorEntityLength * 2, Allocator.Persistent);
            longRunningDisposables.Add(seedPoints);
            newJob = Job
                .WithBurst()
                .WithDisposeOnCompletion(actorSeeds)
                .WithCode(() =>
                {
                    for (int i = 0; i < actorSeeds.Length; i++)
                    {
                        seedPoints[i] = actorSeeds[i];
                    }
                    for (int i = 0; i < anchors.Length; i++)
                    {
                        seedPoints[i + actorSeeds.Length] = anchors[i].destinationCoordinate;
                        seedPoints[i + actorSeeds.Length + anchors.Length] = anchorPositions[i].Value;
                    }
                })
                .Schedule(dependency);
            return seedPoints;
        }

        private UniversalCoordinate[] GetActorPositions()
        {
            return UnityEngine.Object.FindObjectsOfType<TileMapNavigationMember>()
                .Select(x => x.CoordinatePosition)
                .ToArray();
        }

        private JobHandle ScheduleRegionClassificationJobs(
            NativeArray<UniversalCoordinateRange> ranges,
            NativeArray<UniversalCoordinate> seedPoints,
            NativeHashSet<UniversalCoordinate> impassibleCoordiantes,
            NativeHashMap<UniversalCoordinate, int> outputRegionIndexes,
            out NativeList<AllocatedRegion> allocatedRegions,
            out NativeArray<int> regionCounter,
            JobHandle dependency = default)
        {
            Profiler.BeginSample("Schedule: region classification");

            allocatedRegions = new NativeList<AllocatedRegion>(ranges.Length, Allocator.Persistent);
            longRunningDisposables.Add(allocatedRegions);
            regionCounter = new NativeArray<int>(1, Allocator.Persistent);
            regionCounter[0] = 0;
            longRunningDisposables.Add(regionCounter);
            foreach (var coordinateRange in ranges)
            {
                // inputs have to be persistent, this job is long-running
                var regionClassifierJob = new CoordinateRangeToRegionMapJob
                {
                    coordinateRangeToIterate_input = coordinateRange,
                    impassableTiles_input = impassibleCoordiantes,
                    seedPoints_input = seedPoints,
                    fringe_working = new NativeQueue<UniversalCoordinate>(Allocator.Persistent),
                    neighborCoordinatesSwapSpace_working = new NativeArray<UniversalCoordinate>(UniversalCoordinate.MaxNeighborCount, Allocator.Persistent),
                    regionIndexes_output = outputRegionIndexes,
                    allRegions_output = allocatedRegions,
                    regionCounter_working = regionCounter
                };

                dependency = regionClassifierJob.Schedule(dependency);
                longRunningDisposables.Add(regionClassifierJob.fringe_working);
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
            var memberManager = CombinationTileMapManager.instance.everyMember;
            var tileTypeDict = memberManager.GetTileTypesByCoordinateReadonlyCollection();
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
                    CoordinateRangeJobsBatchSize,
                    dependency);
            }
            memberManager.RegisterJobHandleForReader(dependency);

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
