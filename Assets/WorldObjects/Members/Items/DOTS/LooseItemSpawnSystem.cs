using Assets.Tiling;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Food.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

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
            if(SpawnCommandsQuery.IsEmpty)
            {
                return;
            }
            var spawnCommandEntities = SpawnCommandsQuery.ToEntityArrayAsync(Allocator.TempJob, out var supplyEntityJob);
            var spawnCommands = SpawnCommandsQuery.ToComponentDataArrayAsync<LooseItemSpawnCommandComponent>(Allocator.TempJob, out var spawnCommandJob);
            var spawnCommandPositons = SpawnCommandsQuery.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.TempJob, out var spawnPositionJob);
            Dependency = JobHandle.CombineDependencies(
                JobHandle.CombineDependencies(supplyEntityJob, spawnCommandJob, spawnPositionJob),
                Dependency);

            var commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Dependency = Entities
                .WithDisposeOnCompletion(spawnCommandEntities)
                .WithDisposeOnCompletion(spawnCommands)
                .WithDisposeOnCompletion(spawnCommandPositons)
                .ForEach((int entityInQueryIndex, Entity entity, in LooseItemPrefabComponent prefab) =>
            {
                for (int i = 0; i < spawnCommands.Length; i++)
                {
                    var command = spawnCommands[i];
                    if(command.type == prefab.type)
                    {
                        var spawnedEntity = commandBuffer.Instantiate(entityInQueryIndex, prefab.looseItemPrefab);
                        var amountBuffer = commandBuffer.SetBuffer<ItemAmountClaimBufferData>(entityInQueryIndex, spawnedEntity);
                        amountBuffer.Add(new ItemAmountClaimBufferData
                        {
                            Type = command.type,
                            Amount = command.amount,
                            TotalSubtractionClaims = 0f
                        });

                        commandBuffer.SetComponent(entityInQueryIndex, spawnedEntity, new UniversalCoordinatePositionComponent
                        {
                            Value = spawnCommandPositons[i].Value
                        });

                        commandBuffer.DestroyEntity(entityInQueryIndex, spawnCommandEntities[i]);
                    }
                }
            }).ScheduleParallel(Dependency);

            commandBufferSystem.AddJobHandleForProducer(Dependency);
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
