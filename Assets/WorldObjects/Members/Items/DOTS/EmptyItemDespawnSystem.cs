using Assets.WorldObjects.Members.Storage.DOTS;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EmptyItemDespawnSystem : SystemBase
    {
        EntityCommandBufferSystem despawnCommandBuffer => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            var commandBuffer = despawnCommandBuffer.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<LooseItemFlagComponent>()
                .ForEach((int entityInQueryIndex, Entity self, in DynamicBuffer<ItemAmountClaimBufferData> amountBuffer) =>
            {
                if (amountBuffer.TotalAmounts() <= 0)
                {
                    commandBuffer.DestroyEntity(entityInQueryIndex, self);
                }
            }).ScheduleParallel();
            despawnCommandBuffer.AddJobHandleForProducer(Dependency);
        }

    }
}
