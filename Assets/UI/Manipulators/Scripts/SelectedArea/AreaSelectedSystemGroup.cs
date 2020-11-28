using Assets.UI.ThingSelection.ClickSelector;
using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;
using UnityEngine;

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
            Debug.Log("group updated");
            var buffer = commandBufferSystem.CreateCommandBuffer();
            buffer.DestroyEntity(completedDragQuery);
        }
    }
}
