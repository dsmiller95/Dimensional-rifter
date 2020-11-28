using Assets.Scripts.DOTS.ErrandClaims;
using Assets.Tiling;
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
    public class SpecificResourceConsumeErrandRequestSystem : ErrandRequestSystem<SpecificResourceConsumeRequestComponent, SpecificResourceErrandResultComponent>
    {
        protected override bool PreCheckRequest(in SpecificResourceConsumeRequestComponent requestData)
        {
            return requestData.DataIsSet;
        }

        protected override void CheckJob(
            Entity requestEntity,
            SpecificResourceConsumeRequestComponent requestData,
            uint requestRegion,
            NativeHashMap<UniversalCoordinate, uint> regionMap,
            EntityCommandBuffer commandBuffer)
        {
            var didSetResult = new NativeArray<bool>(new[] { false }, Allocator.TempJob);
            Entities
                .WithReadOnly(regionMap)
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
                    if ((itemType.SourceTypeFlag & requestData.ItemSourceTypeFlags) == 0)
                    {
                        return;
                    }
                    if (!regionMap.TryGetValue(position.Value, out var itemSourceRegion) || (itemSourceRegion & requestRegion) == 0)
                    {
                        return;
                    }

                    for (var itemAmountIndex = 0; itemAmountIndex < itemAmountBuffer.Length; itemAmountIndex++)
                    {
                        var itemAmount = itemAmountBuffer[itemAmountIndex];
                        if (requestData.resourceToConsume != itemAmount.Type)
                        {
                            continue;
                        }
                        var totalClaimableAmount = itemAmount.Amount - itemAmount.TotalSubtractionClaims;
                        if (totalClaimableAmount <= 0)
                        {
                            return; // should only be one buffer data with the same resource type, so we can return out here
                        }

                        var amountToConsume = math.min(requestData.maxResourceConsume, totalClaimableAmount);
                        itemAmount.TotalSubtractionClaims += amountToConsume;
                        itemAmountBuffer[itemAmountIndex] = itemAmount;
                        var errandResult = new SpecificResourceErrandResultComponent
                        {
                            amountToConsume = amountToConsume,
                            consumeTarget = self,
                            resourceType = requestData.resourceToConsume
                        };
                        commandBuffer.AddComponent(requestEntity, errandResult);
                        didSetResult[0] = true;
                        return;
                    }
                }).Schedule();
            Job.WithBurst().WithDisposeOnCompletion(didSetResult).WithCode(() =>
            {
                if (!didSetResult[0])
                    commandBuffer.DestroyEntity(requestEntity);
            }).Schedule();
        }
    }
}
