using Unity.Entities;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{

    public class ConnectivityEntitySystem : SystemBase
    {
        private EntityQuery BlockingEntities;

        protected override void OnCreate()
        {
        }

        protected override void OnUpdate()
        {
            //var blockedPositionsIndexed = new NativeHashSet<UniversalCoordinate>(BlockingEntities.CalculateEntityCount(), Allocator.TempJob);

            //var concurrentWriter = blockedPositionsIndexed.AsParallelWriter();
            //Entities
            //    .WithStoreEntityQueryInField(ref BlockingEntities)
            //    .ForEach((int entityInQueryIndex, Entity self, in TileBlockingComponent blocking, in UniversalCoordinatePositionComponent position) =>
            //{
            //    if (blocking.CurrentlyBlocking)
            //    {
            //        concurrentWriter.Add(position.Value);
            //    }
            //}).ScheduleParallel();

            //var ranges = CombinationTileMapManager.instance.allRegions.Select(data => data.baseRange).ToArray();
        }
    }
}
