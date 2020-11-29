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
    [UpdateBefore(typeof(AreaSelectedSystemGroup))]
    public class DragSelectAreaVisualizerSystem : SystemBase
    {
        EntityCommandBufferSystem commandBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        EntityQuery dragEventQuery;
        EntityQuery dragCompleteQuery;

        EntityQuery visualizerPrefabQuery;
        EntityQuery visualizerSpawnedQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            dragEventQuery = GetEntityQuery(
                ComponentType.ReadOnly<DragEventComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>());
            dragEventQuery.AddChangedVersionFilter(typeof(DragEventComponent));
            RequireForUpdate(dragEventQuery);

            dragCompleteQuery = GetEntityQuery(
                ComponentType.ReadOnly<DragEventComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.ReadOnly<DragEventCompleteFlagComponent>()
                );

            visualizerPrefabQuery = GetEntityQuery(typeof(SelectedAreaVisualizerPrefabComponent));
            visualizerSpawnedQuery = GetEntityQuery(typeof(SelectedAreaVisualizerFlagComponent));
            Enabled = false;
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

            if (!enabled)
                commandBuffer.Value.DestroyEntity(visualizerSpawnedQuery);
        }

        protected override void OnUpdate()
        {
            if (!dragCompleteQuery.IsEmpty)
            {
                // if the query is completely empty; then stop showing the range
                //  otherwise check for changes
                var commandBuffer = commandBufferSystem.CreateCommandBuffer();
                commandBuffer.DestroyEntity(visualizerSpawnedQuery);
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
            if (visualizerSpawnedQuery.IsEmpty && !visualizerPrefabQuery.IsEmpty)
            {
                var prefab = visualizerPrefabQuery.GetSingleton<SelectedAreaVisualizerPrefabComponent>();
                enableCommandBuffer.Instantiate(prefab.Value);
            }

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
