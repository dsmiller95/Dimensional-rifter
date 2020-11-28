using Assets.Tiling;
using Assets.UI.Manipulators.Scripts;
using Assets.UI.ThingSelection.ClickSelector;
using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    [UpdateInGroup(typeof(AreaSelectedSystemGroup))]
    public class DeconstructSelectedAreaSystem : SystemBase
    {
        EntityCommandBufferSystem commandBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        private EntityQuery dragEventQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            dragEventQuery = GetEntityQuery(
                ComponentType.ReadOnly<DragEventCompleteFlagComponent>(),
                ComponentType.ReadOnly<DragEventComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>());
        }

        protected override void OnUpdate()
        {
            var dragEventPoint = dragEventQuery.GetSingleton<DragEventComponent>();
            var dragEventOrigin = dragEventQuery.GetSingleton<UniversalCoordinatePositionComponent>();
            var dragRange = UniversalCoordinateRange.From(dragEventOrigin.Value, dragEventPoint.dragPos);
            if (!dragRange.IsValid)
            {
                return;
            }

            var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithNone<DeconstructBuildingClaimComponent>()
                .ForEach((
                    int entityInQueryIndex,
                    Entity self,
                    in BuildingParentComponent building,
                    in UniversalCoordinatePositionComponent position) =>
                {
                    if (building.isBuilt && dragRange.ContainsCoordinate(position.Value))
                    {
                        commandBuffer.AddComponent<DeconstructBuildingClaimComponent>(entityInQueryIndex, self);
                    }
                }).ScheduleParallel();

            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
