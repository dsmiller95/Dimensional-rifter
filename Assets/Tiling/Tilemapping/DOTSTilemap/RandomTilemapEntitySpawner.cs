using Assets.Tiling.SquareCoords;
using Assets.WorldObjects.DOTSMembers;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.DOTSTilemap
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class RandomTilemapEntitySpawner : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var time = UnityEngine.Time.time;

            Entities
                .ForEach((int entityInQueryIndex,
                    ref RandomProviderComponent randomProvider,
                    ref TilemapSpawnerComponent spawner,
                    in MemberPrefabComponent entityPrefab) =>
                {
                    if (spawner.nextSpawnTime < time)
                    {
                        spawner.nextSpawnTime = time + spawner.timePerSpawn;
                        var randCoord = spawner.spawningRange.GetRandomCoordinate(ref randomProvider.value);
                        var newCoordinate = UniversalCoordinate.From(randCoord, spawner.planeIndex);

                        var newEntity = commandBuffer.Instantiate(entityInQueryIndex, entityPrefab.prefab);

                        commandBuffer.SetComponent(entityInQueryIndex, newEntity, new UniversalCoordinatePosition
                        {
                            coordinate = newCoordinate
                        });
                    }
                })
                .WithName("SpawnTilemapMembers")
                .ScheduleParallel();
            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
