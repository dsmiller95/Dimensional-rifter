using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Buildings.DOTS.DeconstructErrand
{
    public struct DeconstructErrandRequestComponent : IComponentData
    {
        public bool DataIsSet;
    }

    public struct DeconstructErrandResultComponent : IComponentData
    {
        public Entity deconstructTarget;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class DeconstructErrandRequestSystem : SystemBase
    {
        private EntityQuery SleepErrandRequest;

        protected override void OnCreate()
        {
            SleepErrandRequest = GetEntityQuery(
                ComponentType.ReadOnly<DeconstructErrandRequestComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.Exclude<DeconstructErrandResultComponent>()
                );
        }

        EntityCommandBufferSystem finishedRequestBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        ConnectivityEntitySystem ConnectivitySystem => World.GetOrCreateSystem<ConnectivityEntitySystem>();
        protected override void OnUpdate()
        {
            var connectionSystem = ConnectivitySystem;
            if (SleepErrandRequest.IsEmpty || !connectionSystem.HasRegionMaps)
            {
                return;
            }
            var errandEntities = SleepErrandRequest.ToEntityArrayAsync(Allocator.TempJob, out var entityJob);
            var errandRequests = SleepErrandRequest.ToComponentDataArrayAsync<DeconstructErrandRequestComponent>(Allocator.TempJob, out var errandJob);
            var errandPositions = SleepErrandRequest.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.TempJob, out var originPositionJob);
            var dataGrab = JobHandle.CombineDependencies(entityJob, errandJob, originPositionJob);
            dataGrab.Complete();

            var commandBuffer = finishedRequestBufferSystem.CreateCommandBuffer();
            var regionMap = connectionSystem.Regions;

            for (var errandIndex = 0; errandIndex < errandRequests.Length; errandIndex++)
            {
                var errandRequest = errandRequests[errandIndex];
                if (!errandRequest.DataIsSet)
                {
                    continue;
                }
                if (!regionMap.TryGetValue(errandPositions[errandIndex].Value, out var requestOriginRegion))
                {
                    continue; // if request is not in a mapped region, skip
                }
                var errandEntity = errandEntities[errandIndex];
                var didSetResult = new NativeArray<bool>(new[] { false }, Allocator.TempJob);
                Entities
                    .WithReadOnly(regionMap)
                    .ForEach((int entityInQueryIndex, Entity self,
                        ref DeconstructBuildingClaimComponent errandClaimed,
                        in UniversalCoordinatePositionComponent position) =>
                    {
                        if (didSetResult[0] || errandClaimed.DeconstructClaimed)
                        {
                            return;
                        }
                        if (!regionMap.TryGetValue(position.Value, out var itemSourceRegion) || (itemSourceRegion & requestOriginRegion) == 0)
                        {
                            return;
                        }
                        errandClaimed.DeconstructClaimed = true;
                        commandBuffer.AddComponent(errandEntity, new DeconstructErrandResultComponent
                        {
                            deconstructTarget = self
                        });
                        didSetResult[0] = true;
                    }).Schedule();
                Dependency = Job
                    .WithBurst()
                    .WithDisposeOnCompletion(didSetResult)
                    .WithCode(() =>
                    {
                        if (!didSetResult[0])
                        {
                            commandBuffer.DestroyEntity(errandEntity);
                        }
                    })
                    .Schedule(Dependency);
            }

            finishedRequestBufferSystem.AddJobHandleForProducer(Dependency);

            Dependency = JobHandle.CombineDependencies(
                errandPositions.Dispose(Dependency),
                errandEntities.Dispose(Dependency),
                errandRequests.Dispose(Dependency)
                );
        }
    }
}
