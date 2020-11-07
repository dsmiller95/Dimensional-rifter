using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EmptyItemDespaynSystem : SystemBase
    {
        EntityCommandBufferSystem despawnCommandBuffer => World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            var commandBuffer = despawnCommandBuffer.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithAll<LooseItemFlagComponent>()
                .ForEach((int entityInQueryIndex, Entity self, ref ItemAmountComponent amount) =>
            {
                if (amount.resourceAmount <= 0)
                {
                    commandBuffer.DestroyEntity(entityInQueryIndex, self);
                }
            }).ScheduleParallel();
            despawnCommandBuffer.AddJobHandleForProducer(Dependency);
        }

    }
}
