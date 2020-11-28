using Assets.UI.ThingSelection.ClickSelector;
using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;

namespace Assets.UI.Manipulators.Scripts
{
    /// <summary>
    /// Runs only when there is a completed drag event; and removes the drag event on the next frame with the commandBuffer
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class AreaSelectedSystemGroup : ComponentSystemGroup
    {
        EntityCommandBufferSystem commandBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        EntityQuery completedDragQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            completedDragQuery = GetEntityQuery(
                ComponentType.ReadOnly(typeof(DragEventCompleteFlagComponent)),
                ComponentType.ReadOnly(typeof(DragEventComponent)),
                ComponentType.ReadOnly(typeof(UniversalCoordinatePositionComponent))
                );
            RequireForUpdate(completedDragQuery);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            var buffer = commandBufferSystem.CreateCommandBuffer();
            buffer.DestroyEntity(completedDragQuery);
        }

        public void DisableSystemsInGroup()
        {
            foreach (var system in Systems)
            {
                system.Enabled = false;
            }
        }
    }
}
