using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class BuildErrandActivateSystem : SystemBase
    {
        EntityCommandBufferSystem despawnCommandBuffer => World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            var commandBuffer = despawnCommandBuffer.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<IsNotBuiltFlag>()
                .WithNone<BuildErrandTargetComponent>()
                .ForEach((
                    int entityInQueryIndex,
                    Entity self,
                    in ItemAmountsDataComponent itemAmountData,
                    in DynamicBuffer<ItemAmountClaimBufferData> amountBuffer) =>
            {
                if(amountBuffer.TotalAmounts() >= itemAmountData.MaxCapacity)
                {
                    commandBuffer.AddComponent<BuildErrandTargetComponent>(entityInQueryIndex, self);
                }
            }).ScheduleParallel();
            despawnCommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}
