using Assets.Tiling;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Food.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.WorldObjects.Members.Items.DOTS
{

    public class LooseItemSpawnSystem : SystemBase
    {
        private EntityArchetype looseItemCommandArchetype;
        private EntityQuery SpawnCommandsQuery;

        protected override void OnCreate()
        {
            looseItemCommandArchetype = EntityManager.CreateArchetype(
                typeof(UniversalCoordinatePositionComponent),
                typeof(LooseItemSpawnCommandComponent)
                );
            SpawnCommandsQuery = GetEntityQuery(
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.ReadOnly<LooseItemSpawnCommandComponent>()
                );
        }

        EntityCommandBufferSystem commandBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            if (SpawnCommandsQuery.IsEmpty)
            {
                return;
            }
            var spawnCommandEntities = SpawnCommandsQuery.ToEntityArrayAsync(Allocator.TempJob, out var supplyEntityJob);
            var spawnCommands = SpawnCommandsQuery.ToComponentDataArrayAsync<LooseItemSpawnCommandComponent>(Allocator.TempJob, out var spawnCommandJob);
            var spawnCommandPositons = SpawnCommandsQuery.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.TempJob, out var spawnPositionJob);
            var dataDep = JobHandle.CombineDependencies(supplyEntityJob, spawnCommandJob, spawnPositionJob);
            Dependency = JobHandle.CombineDependencies(
                dataDep,
                Dependency);

            var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            dataDep.Complete();
            for (int spawnCommandIndex = 0; spawnCommandIndex < spawnCommandEntities.Length; spawnCommandIndex++)
            {
                // there should only ever be one matching looseItemPrefabComponent, if there are multiple then multiple items will be spawned
                var command = spawnCommands[spawnCommandIndex];
                var commandPosition = spawnCommandPositons[spawnCommandIndex];
                var commandEntity = spawnCommandEntities[spawnCommandIndex];
                var amountLeftToSpawn = new NativeArray<float>(new[] { command.amount }, Allocator.TempJob);
                Entities
                    .WithAll<LooseItemFlagComponent>()
                    .ForEach((int entityInQueryIndex, Entity entity,
                        ref DynamicBuffer<ItemAmountClaimBufferData> itemBufferData,
                        in ItemAmountsDataComponent itemCapacityData,
                        in UniversalCoordinatePositionComponent position) =>
                    {
                        if (amountLeftToSpawn[0] <= 0)
                        {
                            return;
                        }
                        if (position.Value != commandPosition.Value)
                        {
                            return;
                        }
                        var indexForItem = itemBufferData.IndexOfType(command.type);
                        if (indexForItem < 0)
                        {
                            return;
                        }
                        var freeSpace = itemCapacityData.MaxCapacity - itemBufferData.TotalAmounts() - itemCapacityData.TotalAdditionClaims;

                        var amountToAdd = math.min(freeSpace, amountLeftToSpawn[0]);

                        var itemAmount = itemBufferData[indexForItem];
                        itemAmount.Amount += amountToAdd;
                        itemBufferData[indexForItem] = itemAmount;
                        amountLeftToSpawn[0] -= amountToAdd;
                        if (amountLeftToSpawn[0] <= 0)
                        {
                            commandBuffer.DestroyEntity(entityInQueryIndex, spawnCommandEntities[spawnCommandIndex]);
                        }
                    }).Schedule();
                Entities
                    .WithDisposeOnCompletion(amountLeftToSpawn)
                    .ForEach((int entityInQueryIndex, Entity entity, in LooseItemPrefabComponent prefab) =>
                    {
                        if (amountLeftToSpawn[0] <= 0)
                        {
                            return;
                        }
                        if (command.type == prefab.type)
                        {
                            var spawnedEntity = commandBuffer.Instantiate(entityInQueryIndex, prefab.looseItemPrefab);
                            var amountBuffer = commandBuffer.SetBuffer<ItemAmountClaimBufferData>(entityInQueryIndex, spawnedEntity);
                            amountBuffer.Add(new ItemAmountClaimBufferData
                            {
                                Type = command.type,
                                Amount = amountLeftToSpawn[0],
                                TotalSubtractionClaims = 0f
                            });

                            commandBuffer.SetComponent(entityInQueryIndex, spawnedEntity, new UniversalCoordinatePositionComponent
                            {
                                Value = spawnCommandPositons[spawnCommandIndex].Value
                            });

                            commandBuffer.DestroyEntity(entityInQueryIndex, spawnCommandEntities[spawnCommandIndex]);
                        }
                    }).ScheduleParallel();
            }

            commandBufferSystem.AddJobHandleForProducer(Dependency);

            Dependency = JobHandle.CombineDependencies(
                spawnCommandEntities.Dispose(Dependency),
                spawnCommands.Dispose(Dependency),
                spawnCommandPositons.Dispose(Dependency)
                );
        }

        public void SpawnLooseItem(UniversalCoordinate postion, GrowthProductComponent growthData, EntityCommandBuffer buffer)
        {
            SpawnLooseItem(
                postion,
                growthData.grownResource,
                growthData.resourceAmount,
                buffer
                );
        }

        public void SpawnLooseItem(
            UniversalCoordinate position,
            Resource resource,
            float amount,
            EntityCommandBuffer commandbuffer)
        {
            var entity = commandbuffer.CreateEntity(looseItemCommandArchetype);
            commandbuffer.SetComponent(entity, new UniversalCoordinatePositionComponent
            {
                Value = position
            });

            commandbuffer.SetComponent(entity, new LooseItemSpawnCommandComponent
            {
                type = resource,
                amount = amount
            });
        }
    }
}
