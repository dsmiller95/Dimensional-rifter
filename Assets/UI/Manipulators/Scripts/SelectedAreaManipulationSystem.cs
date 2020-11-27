using Assets.UI.ThingSelection.ClickSelector;
using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class SelectedAreaManipulationSystem : SystemBase
    {
        EntityCommandBufferSystem commandBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        EntityQuery completedDragQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = commandBufferSystem.CreateCommandBuffer();
            Debug.Log("Doin a manipulate");
            Entities
                .WithAll<DragEventCompleteFlagComponent>()
                .WithStoreEntityQueryInField(ref completedDragQuery)
                .ForEach((Entity self, in DragEventComponent dragEvent, in UniversalCoordinatePositionComponent position) =>
                {

                }).Schedule();
            commandBuffer.DestroyEntity(completedDragQuery);
        }
    }
}
