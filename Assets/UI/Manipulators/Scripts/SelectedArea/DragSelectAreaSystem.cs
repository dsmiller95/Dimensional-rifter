using Assets.UI.ThingSelection.ClickSelector;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.UI.Manipulators.Scripts.SelectedArea
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DragSelectAreaSystem : SystemBase
    {
        EntityCommandBufferSystem commandBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        EntityQuery dragEventQuery;
        EntityQuery completedDragQuery;

        EntityQuery disabledVisualizerQuery;
        EntityQuery enabledVisualizerQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            dragEventQuery = GetEntityQuery(
                ComponentType.ReadOnly<DragEventComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>());
            dragEventQuery.AddChangedVersionFilter(typeof(DragEventComponent));
            RequireForUpdate(dragEventQuery);

            completedDragQuery = GetEntityQuery(
                ComponentType.ReadOnly<DragEventComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.ReadOnly<DragEventCompleteFlagComponent>()
                );

            disabledVisualizerQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<SelectedAreaVisualizerFlagComponent>(), ComponentType.ReadOnly<Disabled>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
            enabledVisualizerQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<SelectedAreaVisualizerFlagComponent>() }
            });
        }

        public void SetVisualizerEnabled(bool enabled, EntityCommandBuffer? commandBuffer = null)
        {
            if (Enabled == enabled)
            {
                return;
            }
            if (!commandBuffer.HasValue)
                commandBuffer = commandBufferSystem.CreateCommandBuffer();
            Enabled = enabled;
            SetQueryEnabled(enabled, commandBuffer.Value, disabledVisualizerQuery, enabledVisualizerQuery);
        }

        private static void SetQueryEnabled(bool visible, EntityCommandBuffer commandBuffer, EntityQuery disabledQuery, EntityQuery enabledQuery)
        {
            if (visible == true)
            {
                commandBuffer.RemoveComponent<Disabled>(disabledQuery);
            }
            else
            {
                commandBuffer.AddComponent<Disabled>(enabledQuery);
            }
        }

        protected override void OnUpdate()
        {
            if (!completedDragQuery.IsEmpty)
            {
                var commandBuffer = commandBufferSystem.CreateCommandBuffer();
                // TODO: remove this when there is another system which actually does something when the selection is complete
                commandBuffer.DestroyEntity(completedDragQuery);
                SetQueryEnabled(false, commandBuffer, disabledVisualizerQuery, enabledVisualizerQuery);
                return;
            }
            // The RequireForUpdate ignores changed version filters completely; so this check will make sure that
            //  the rest of the job only runs on changes
            if (dragEventQuery.IsEmpty)
            {
                return;
            }
            var dragEntities = dragEventQuery.ToEntityArrayAsync(Allocator.TempJob, out var dragDataJob);
            var center = new NativeArray<float2>(1, Allocator.TempJob);
            var scale = new NativeArray<float3>(1, Allocator.TempJob);

            var enableCommandBuffer = commandBufferSystem.CreateCommandBuffer();
            SetQueryEnabled(true, enableCommandBuffer, disabledVisualizerQuery, enabledVisualizerQuery);

            dragDataJob = Job
                .WithBurst()
                .WithDisposeOnCompletion(dragEntities)
                .WithCode(() =>
                {
                    var activeEntity = dragEntities[0];
                    var rootPos = GetComponent<UniversalCoordinatePositionComponent>(activeEntity);
                    var dragPos = GetComponent<DragEventComponent>(activeEntity);
                    var root = rootPos.Value.ToPositionInPlane();
                    var extent = dragPos.dragPos.ToPositionInPlane();
                    if (root.x > extent.x)
                    {
                        var swap = root.x;
                        root.x = extent.x;
                        extent.x = swap;
                    }
                    if (root.y > extent.y)
                    {
                        var swap = root.y;
                        root.y = extent.y;
                        extent.y = swap;
                    }
                    center[0] = (root + extent) / 2;
                    scale[0] = new float3((extent - root) + new float2(1, 1), 1);
                }).Schedule(dragDataJob);

            Dependency = Entities
                .WithDisposeOnCompletion(center)
                .WithDisposeOnCompletion(scale)
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
                .WithAll<SelectedAreaVisualizerFlagComponent, Translation, NonUniformScale>()
                .ForEach((Entity entity, in Translation trans) =>
                {
                    enableCommandBuffer.SetComponent(entity, new Translation
                    {
                        Value = new float3(center[0], trans.Value.z)
                    });
                    enableCommandBuffer.SetComponent(entity, new NonUniformScale
                    {
                        Value = scale[0]
                    });
                }).Schedule(JobHandle.CombineDependencies(dragDataJob, Dependency));

            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
