using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.WorldObjects.Members.Hungry.DOTS.EatingErrand
{
    public struct SpecificResourceConsumeRequestComponent : IComponentData
    {
        public bool DataIsSet;
        public uint ItemSourceTypeFlags;
        public Resource resourceToConsume; // TODO: eventually use some sort of flags or property-based filtering
        public float maxResourceConsume;
    }

    public struct SpecificResourceErrandResultComponent : IComponentData
    {
        public Entity consumeTarget;
        public Resource resourceType;
        public float amountToConsume;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class SpecificResourceConsumeErrandRequestSystem : SystemBase
    {
        private EntityQuery ConsumeErrandRequest;

        protected override void OnCreate()
        {
            ConsumeErrandRequest = GetEntityQuery(
                ComponentType.ReadOnly<SpecificResourceConsumeRequestComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.Exclude<SpecificResourceErrandResultComponent>()
                );
        }

        EntityCommandBufferSystem finishedRequestBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        ConnectivityEntitySystem ConnectivitySystem => World.GetOrCreateSystem<ConnectivityEntitySystem>();
        protected override void OnUpdate()
        {
            var connectionSystem = ConnectivitySystem;
            if (ConsumeErrandRequest.IsEmpty || !connectionSystem.HasRegionMaps)
            {
                return;
            }
            var errandEntities = ConsumeErrandRequest.ToEntityArrayAsync(Allocator.TempJob, out var entityJob);
            var errandRequests = ConsumeErrandRequest.ToComponentDataArrayAsync<SpecificResourceConsumeRequestComponent>(Allocator.TempJob, out var errandJob);
            var errandPositions = ConsumeErrandRequest.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.TempJob, out var originPositionJob);
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
                        ref DynamicBuffer<ItemAmountClaimBufferData> itemAmountBuffer,
                        in UniversalCoordinatePositionComponent position,
                        in ItemSourceTypeComponent itemType,
                        in ItemAmountsDataComponent amount) =>
                    {
                        if (didSetResult[0])
                        {
                            return;
                        }
                        if ((itemType.SourceTypeFlag & errandRequest.ItemSourceTypeFlags) == 0)
                        {
                            return;
                        }
                        if (!regionMap.TryGetValue(position.Value, out var itemSourceRegion) || (itemSourceRegion & requestOriginRegion) == 0)
                        {
                            return;
                        }

                        for (var itemAmountIndex = 0; itemAmountIndex < itemAmountBuffer.Length; itemAmountIndex++)
                        {
                            var itemAmount = itemAmountBuffer[itemAmountIndex];
                            if (errandRequest.resourceToConsume != itemAmount.Type)
                            {
                                continue;
                            }
                            var totalClaimableAmount = itemAmount.Amount - itemAmount.TotalSubtractionClaims;
                            if (totalClaimableAmount <= 0)
                            {
                                return; // should only be one buffer data with the same resource type, so we can return out here
                            }

                            var amountToConsume = math.min(errandRequest.maxResourceConsume, totalClaimableAmount);
                            itemAmount.TotalSubtractionClaims += amountToConsume;
                            itemAmountBuffer[itemAmountIndex] = itemAmount;
                            var errandResult = new SpecificResourceErrandResultComponent
                            {
                                amountToConsume = amountToConsume,
                                consumeTarget = self,
                                resourceType = errandRequest.resourceToConsume
                            };
                            commandBuffer.AddComponent(errandEntity, errandResult);
                            didSetResult[0] = true;
                            return;
                        }
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
