using Assets.Tiling;
using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.DOTS.ErrandClaims
{

    public abstract class ErrandRequestSystem<Request, Response> : SystemBase
        where Request : unmanaged, IComponentData
        where Response : unmanaged, IComponentData
    {
        private EntityQuery ErrandRequestQuery;
        EntityCommandBufferSystem finishedRequestBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        ConnectivityEntitySystem ConnectivitySystem => World.GetOrCreateSystem<ConnectivityEntitySystem>();

        protected override void OnCreate()
        {
            ErrandRequestQuery = GetEntityQuery(
                ComponentType.ReadOnly<Request>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.Exclude<Response>()
                );
        }

        protected override void OnUpdate()
        {
            var connectionSystem = ConnectivitySystem;
            if (ErrandRequestQuery.IsEmpty || !connectionSystem.HasRegionMaps)
            {
                return;
            }
            var errandEntities = ErrandRequestQuery.ToEntityArrayAsync(Allocator.TempJob, out var entityJob);
            var errandRequests = ErrandRequestQuery.ToComponentDataArrayAsync<Request>(Allocator.TempJob, out var errandJob);
            var errandPositions = ErrandRequestQuery.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.TempJob, out var originPositionJob);
            var dataGrab = JobHandle.CombineDependencies(entityJob, errandJob, originPositionJob);
            dataGrab.Complete();

            var commandBuffer = finishedRequestBufferSystem.CreateCommandBuffer();
            var regionMap = connectionSystem.Regions;

            for (var errandIndex = 0; errandIndex < errandRequests.Length; errandIndex++)
            {
                var errandRequest = errandRequests[errandIndex];
                if (!PreCheckRequest(in errandRequest))
                {
                    continue;
                }
                if (!regionMap.TryGetValue(errandPositions[errandIndex].Value, out var requestOriginRegion))
                {
                    continue; // if request is not in a mapped region, skip
                }
                var errandEntity = errandEntities[errandIndex];
                CheckJob(
                    errandEntity,
                    errandRequest,
                    requestOriginRegion,
                    regionMap,
                    commandBuffer);
            }

            finishedRequestBufferSystem.AddJobHandleForProducer(Dependency);

            Dependency = JobHandle.CombineDependencies(
                errandPositions.Dispose(Dependency),
                errandEntities.Dispose(Dependency),
                errandRequests.Dispose(Dependency)
                );
        }

        protected abstract bool PreCheckRequest(in Request requestData);

        protected abstract void CheckJob(
            Entity requestEntity,
            Request requestData,
            uint requestRegion,
            NativeHashMap<UniversalCoordinate, uint> regionMap,
            EntityCommandBuffer commandBuffer);
    }
}
