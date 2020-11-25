using Unity.Entities;

namespace Assets.WorldObjects.SaveObjects.SaveManager
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class PostDeserialzeSystemGroup : ComponentSystemGroup
    {
        EntityCommandBufferSystem removeFlagBuffer => World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        private EntityQuery DeserializeFlagComponent;
        protected override void OnCreate()
        {
            base.OnCreate();
            DeserializeFlagComponent = GetEntityQuery(
                ComponentType.ReadOnly<DeserializingFlagComponent>()
                );
        }

        protected override void OnUpdate()
        {
            if (!DeserializeFlagComponent.IsEmpty)
            {
                base.OnUpdate();
                var buffer = removeFlagBuffer.CreateCommandBuffer();
                buffer.DestroyEntity(DeserializeFlagComponent);
            }
            else
            {
                Enabled = false;
            }
        }
    }

    public struct DeserializingFlagComponent : IComponentData
    {
    }
}
