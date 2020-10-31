using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Tiling.Tilemapping.DOTSTilemap
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class TilemapSpawningSystem : SystemBase
    {
        EntityCommandBufferSystem spawningCommandSystem => World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = spawningCommandSystem.CreateCommandBuffer().AsParallelWriter();
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
                        var newCoordinate = UniversalCoordinate.From(randCoord, -1);


                        var newEntity = commandBuffer.Instantiate(entityInQueryIndex, entityPrefab.prefab);

                        commandBuffer.SetComponent(entityInQueryIndex, newEntity, new UniversalCoordinatePosition
                        {
                            coordinate = newCoordinate
                        });

                        commandBuffer.AddComponent(entityInQueryIndex, newEntity, new LocalToParent());

                        commandBuffer.AddComponent(entityInQueryIndex, newEntity, new Parent
                        {
                            Value = spawner.spawnedParent
                        });
                    }
                })
                .WithName("SpawnTilemapMembers")
                .ScheduleParallel();
            spawningCommandSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
