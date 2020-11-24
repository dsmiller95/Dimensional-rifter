using Assets.Tiling;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.UI.ThingSelection.ClickSelector
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ClickToSelectedItemSystem : SystemBase
    {
        EntityCommandBufferSystem commandBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private EntityQuery TileClickedEventQuery;
        private EntityQuery ClickableEntityQuery;

        protected override void OnCreate()
        {
            TileClickedEventQuery = GetEntityQuery(
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.ReadOnly<ClickEventComponent>()
                );
            ClickableEntityQuery = GetEntityQuery(
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.ReadOnly<SelectableFlagComponent>()
                );

            clickCoordinate = new NativeArray<UniversalCoordinate>(1, Allocator.Persistent);
            selectionIndexInTile = new NativeArray<int>(new[] { -1 }, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            selectionIndexInTile.Dispose();
            clickCoordinate.Dispose();
            base.OnDestroy();
        }

        private NativeArray<int> selectionIndexInTile;
        private NativeArray<UniversalCoordinate> clickCoordinate;

        protected override void OnUpdate()
        {
            if (TileClickedEventQuery.IsEmpty)
            {
                return;
            }

            var clickPositionAndIndexJob = UpdateSelectionIndexAndCoordinate();
            var entitiesOnTile = GetSelectableEntitiesOnSelectedCoordinate(clickPositionAndIndexJob, out var sortedEntityListJob);

            var commandBuffer = commandBufferSystem.CreateCommandBuffer();
            Dependency = JobHandle.CombineDependencies(
                ClearSelectedComponentsAndClickEvents(Dependency, commandBuffer),
                sortedEntityListJob
                );
            var selectionIndexInTile_LambdaCapture = selectionIndexInTile;
            Job
                .WithBurst()
                .WithDisposeOnCompletion(entitiesOnTile)
                .WithCode(() =>
                {
                    if (entitiesOnTile.Length <= 0)
                    {
                        // nothing is on the clicked tile; selection index is invalid
                        selectionIndexInTile_LambdaCapture[0] = -1;
                        return;
                    }

                    selectionIndexInTile_LambdaCapture[0] = selectionIndexInTile_LambdaCapture[0] % entitiesOnTile.Length;
                    var newlySelectedEntity = entitiesOnTile[selectionIndexInTile_LambdaCapture[0]];
                    commandBuffer.AddComponent<SelectedComponent>(newlySelectedEntity);
                })
                .Schedule();
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        private JobHandle ClearSelectedComponentsAndClickEvents(JobHandle dependency, EntityCommandBuffer commandBuffer)
        {
            // not parallelizing this, since only one item will be selected, and only one click event
            var depResult = Entities
                .WithAll<SelectedComponent>()
                .ForEach((Entity self) =>
                {
                    commandBuffer.RemoveComponent<SelectedComponent>(self);
                })
                .Schedule(dependency);
            return Entities
                .WithAll<ClickEventComponent, UniversalCoordinatePositionComponent>()
                .ForEach((Entity self) =>
                {
                    commandBuffer.DestroyEntity(self);
                })
                .Schedule(depResult);
        }

        /// <summary>
        /// Find all entities matching <see cref="ClickableEntityQuery"/> on the same coordinate as <see cref="clickCoordinate"/>
        /// </summary>
        /// <param name="inputDep">job which populates <see cref="clickCoordinate"/></param>
        /// <param name="jobHandle"></param>
        /// <returns>sorted list of entities on the clicked tile</returns>
        private NativeList<Entity> GetSelectableEntitiesOnSelectedCoordinate(JobHandle inputDep, out JobHandle jobHandle)
        {
            var clickableEntities = ClickableEntityQuery.ToEntityArrayAsync(Allocator.TempJob, out var selectableEntityFilterJob);
            var coordFilterJob = new CoordinateOverlapFilterJob
            {
                coordinateToMatch = clickCoordinate,
                coordinatePositions = GetComponentDataFromEntity<UniversalCoordinatePositionComponent>(true),
                entities = clickableEntities
            };

            var indexesOfItemsOnTile = new NativeList<int>(Allocator.TempJob);
            selectableEntityFilterJob = coordFilterJob.ScheduleAppend(
                indexesOfItemsOnTile,
                ClickableEntityQuery.CalculateEntityCount(),
                128,
                JobHandle.CombineDependencies(
                    selectableEntityFilterJob,
                    inputDep
                ));

            return SelectByIndexAndSort(indexesOfItemsOnTile, clickableEntities, out jobHandle, selectableEntityFilterJob);
        }

        /// <summary>
        /// Selects items out of <paramref name="indexedArray"/> based on indexes in <paramref name="indexes"/>, and sorts them based on their natural ordering
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="indexes"></param>
        /// <param name="indexedArray"></param>
        /// <param name="jobHandle"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        private NativeList<Entity> SelectByIndexAndSort(NativeList<int> indexes, NativeArray<Entity> indexedArray, out JobHandle jobHandle, JobHandle dependency)
        {
            var sortedResult = new NativeList<Entity>(Allocator.TempJob);
            jobHandle = Job.WithBurst()
                .WithDisposeOnCompletion(indexes)
                .WithDisposeOnCompletion(indexedArray)
                .WithCode(() =>
                {
                    sortedResult.Resize(indexes.Length, NativeArrayOptions.UninitializedMemory);
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        sortedResult[i] = indexedArray[indexes[i]];
                    }
                    sortedResult.Sort();
                }).Schedule(dependency);
            return sortedResult;
        }

        /// <summary>
        /// Querys <see cref="TileClickedEventQuery"/> events, and uses the last entity in the query to set <see cref="clickCoordinate"/> and <see cref="selectionIndexInTile"/>
        /// </summary>
        /// <returns>Job Handle representing the deffered work</returns>
        private JobHandle UpdateSelectionIndexAndCoordinate()
        {
            var clickEventPositions = TileClickedEventQuery.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.TempJob, out var clickPositionAndIndexJob);

            var clickCoordinate_LambdaCapture = clickCoordinate;
            var selectionIndexInTile_LambdaCapture = selectionIndexInTile;

            clickPositionAndIndexJob = Job
                .WithBurst()
                .WithDisposeOnCompletion(clickEventPositions)
                .WithCode(() =>
                {
                    // only look at the last click in the array; in case there are multiples the last one is most likely to be most recent
                    var clickIndexOfInterest = clickEventPositions.Length - 1;
                    var clickedPosition = clickEventPositions[clickIndexOfInterest].Value;
                    if (clickCoordinate_LambdaCapture[0] == clickedPosition)
                    {
                        selectionIndexInTile_LambdaCapture[0] += 1;
                    }
                    else
                    {
                        selectionIndexInTile_LambdaCapture[0] = 0;
                    }
                    clickCoordinate_LambdaCapture[0] = clickedPosition;
                }).Schedule(clickPositionAndIndexJob);

            return clickPositionAndIndexJob;
        }
    }


    struct CoordinateOverlapFilterJob : IJobParallelForFilter
    {
        [ReadOnly] public NativeArray<UniversalCoordinate> coordinateToMatch;
        [ReadOnly] public ComponentDataFromEntity<UniversalCoordinatePositionComponent> coordinatePositions;
        [ReadOnly] public NativeArray<Entity> entities;
        public bool Execute(int index)
        {
            var entity = entities[index];
            var position = coordinatePositions[entity];
            return position.Value == coordinateToMatch[0];
        }
    }
}
