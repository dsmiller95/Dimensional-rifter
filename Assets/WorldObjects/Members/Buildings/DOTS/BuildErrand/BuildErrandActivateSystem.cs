﻿using Assets.Scripts.DOTS.ErrandClaims;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Buildings.DOTS.BuildErrand
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class BuildErrandActivateSystem : SystemBase
    {
        EntityCommandBufferSystem despawnCommandBuffer => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            var commandBuffer = despawnCommandBuffer.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<IsNotBuiltFlag>()
                .WithNone<ErrandClaimComponent>()
                .ForEach((
                    int entityInQueryIndex,
                    Entity self,
                    in ItemAmountsDataComponent itemAmountData,
                    in DynamicBuffer<ItemAmountClaimBufferData> amountBuffer) =>
            {
                if (amountBuffer.TotalAmounts() >= itemAmountData.MaxCapacity)
                {
                    commandBuffer.AddComponent(entityInQueryIndex, self, new ErrandClaimComponent
                    {
                        Claimed = false
                    });
                }
            }).ScheduleParallel();
            despawnCommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}
