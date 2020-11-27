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
        EntityQuery dragEventQuery;
        EntityQuery dragVisualizerQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            dragEventQuery = GetEntityQuery(
                ComponentType.ReadOnly<DragEventComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.ReadOnly<DragEventStateComponent>());
            dragEventQuery.AddChangedVersionFilter(typeof(DragEventComponent));
            RequireForUpdate(dragEventQuery);
        }

        protected override void OnUpdate()
        {
            // The RequireForUpdate ignores changed version filters completely; so this check will make sure that
            //  the rest of the job only runs on changes
            if (dragEventQuery.IsEmpty)
            {
                return;
            }
            var dragData = dragEventQuery.ToComponentDataArrayAsync<DragEventStateComponent>(Allocator.TempJob, out var dragDataJob);
            var dragEntities = dragEventQuery.ToEntityArrayAsync(Allocator.TempJob, out var dragEntityJob);

            var shouldUpdate = new NativeArray<bool>(new bool[] { false }, Allocator.TempJob);
            var center = new NativeArray<float2>(1, Allocator.TempJob);
            var scale = new NativeArray<float3>(1, Allocator.TempJob);
            dragDataJob = Job
                .WithBurst()
                .WithDisposeOnCompletion(dragData)
                .WithDisposeOnCompletion(dragEntities)
                .WithCode(() =>
                {
                    var activeDragIndex = -1;
                    for (int i = 0; i < dragData.Length; i++)
                    {
                        var dragStatus = dragData[i];
                        if (!dragStatus.dragDone)
                        {
                            activeDragIndex = i;
                            break;
                        }
                    }
                    if (activeDragIndex == -1)
                    {
                        return;
                    }
                    var activeEntity = dragEntities[activeDragIndex];
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
                    shouldUpdate[0] = true;
                }).Schedule(JobHandle.CombineDependencies(dragEntityJob, dragDataJob));


            Dependency = Entities
                .WithDisposeOnCompletion(shouldUpdate)
                .WithDisposeOnCompletion(center)
                .WithDisposeOnCompletion(scale)
                .WithAll<SelectedAreaVisualizerFlagComponent>()
                .WithStoreEntityQueryInField(ref dragVisualizerQuery)
                .ForEach((ref Translation tras, ref NonUniformScale visualizedScale) =>
                {
                    if (!shouldUpdate[0]) return;
                    tras.Value = new float3(center[0], tras.Value.z);
                    visualizedScale.Value = scale[0];
                }).Schedule(JobHandle.CombineDependencies(dragDataJob, Dependency));
        }
    }
}
