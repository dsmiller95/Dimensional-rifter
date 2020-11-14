using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Wall.DOTS;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{

    [AlwaysUpdateSystem]
    public class ConnectivityEntitySystem : SystemBase
    {
        /// <summary>
        /// an estimate of what fraction of tiles will be blocking; used in memory allocation
        /// </summary>
        private static readonly float BlockingTilesRatioEstimate = 0.2f;
        private static readonly int TileTypesToImpassibleQueryBatchSize = 64;

        private EntityQuery BlockingEntities;

        private JobHandle? regionConnectivityDep = null;

        protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
            if (regionConnectivityDep.HasValue && !regionConnectivityDep.Value.IsCompleted)
            {
                return;
            }

            ScheduleNewConnectivityJob();
        }

        private void ScheduleNewConnectivityJob()
        {
            var ranges = new NativeArray<UniversalCoordinateRange>(CombinationTileMapManager.instance.allRegions.Select(data => data.baseRange).ToArray(), Allocator.TempJob);

            var totalCoordinates = ranges.Sum(x => x.TotalCoordinateContents());
            var blockingTilesSizeEstimate = (int)(totalCoordinates * BlockingTilesRatioEstimate);

            var blockedPositionsIndexed = new NativeHashSet<UniversalCoordinate>(blockingTilesSizeEstimate, Allocator.TempJob);

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

            var tileTypes = CombinationTileMapManager.instance.everyMember.GetTileTypesByCoordinateReadonlyCollection().GetKeyValueArrays(Allocator.TempJob);
            var impassibleIDs = GetImpassibleIDSet();

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

            var blockedPositionIndexJobCompletion = tileImpassibleJob;


            blockedPositionIndexJobCompletion = JobHandle.CombineDependencies(
                // TODO: use these to compute region IDs
                ranges.Dispose(blockedPositionIndexJobCompletion),
                blockedPositionsIndexed.Dispose(blockedPositionIndexJobCompletion));

            // keep track of the full dep, don't schedule more until this is complete
            regionConnectivityDep = blockedPositionIndexJobCompletion;

            // only pass through the job which queries the entities
            //  all other jobs will run long
            Dependency = entityBlockingJob;
        }

        private NativeHashSet<int> GetImpassibleIDSet()
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
            var impassibleIDsSet = new NativeHashSet<int>(impassibleTileIDs.Length, Allocator.TempJob);
            foreach (var impassableID in impassibleTileIDs)
            {
                impassibleIDsSet.Add(impassableID);
            }
            return impassibleIDsSet;
        }
    }
}
