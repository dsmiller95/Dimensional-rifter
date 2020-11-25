using Assets.Scripts.DOTS.ErrandClaims;
using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    public struct SleepStationOccupyRequestComponent : IComponentData
    {
        public bool DataIsSet;
    }

    public struct SleepStationOccupyErrandResultComponent : IComponentData
    {
        public Entity sleepTarget;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class SleepStationOccupyErrandRequestSystem : SystemBase
    {
        private EntityQuery SleepErrandRequest;

        protected override void OnCreate()
        {
            SleepErrandRequest = GetEntityQuery(
                ComponentType.ReadOnly<SleepStationOccupyRequestComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.Exclude<SleepStationOccupyErrandResultComponent>()
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
            var errandRequests = SleepErrandRequest.ToComponentDataArrayAsync<SleepStationOccupyRequestComponent>(Allocator.TempJob, out var errandJob);
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
                    .ForEach((int entityInQueryIndex, Entity self,
                        ref ErrandClaimComponent errandClaimed,
                        in SleepStationOccupiedComponent sleepStation,
                        in UniversalCoordinatePositionComponent position) =>
                    {
                        if (didSetResult[0] || errandClaimed.Claimed || sleepStation.Occupied)
                        {
                            return;
                        }
                        if (!regionMap.TryGetValue(position.Value, out var itemSourceRegion) || (itemSourceRegion & requestOriginRegion) == 0)
                        {
                            return;
                        }
                        errandClaimed.Claimed = true;
                        commandBuffer.AddComponent(errandEntity, new SleepStationOccupyErrandResultComponent
                        {
                            sleepTarget = self
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
