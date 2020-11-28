using Assets.Scripts.DOTS.ErrandClaims;
using ECS_SpriteSheetAnimation;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class GrowingThingSystem : SystemBase
    {
        EntityCommandBufferSystem errandAvailabilityCommandSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            // tick up every growing thing that can grow
            Dependency = Entities
                    .WithNone<ErrandClaimComponent>()
                .ForEach((ref GrowingThingComponent growingThing) =>
                {
                    var newGrowth = deltaTime * growingThing.growthPerSecond + growingThing.currentGrowth;
                    growingThing.SetGrownAmount(newGrowth);
                }).ScheduleParallel(Dependency);


            var allRenderMeshes = new List<AnimationRenderMesh>();
            EntityManager.GetAllUniqueSharedComponentData(allRenderMeshes);
            var commandBuffer = errandAvailabilityCommandSystem.CreateCommandBuffer().AsParallelWriter();

            for (var index = 0; index < allRenderMeshes.Count; index++)
            {
                var animationRenderMesh = allRenderMeshes[index];

                var totalFramesInMesh = animationRenderMesh.totalFrames;
                // set animation frame based on growth; and mark all grown growers as having a claimable errand
                Dependency = Entities
                    .WithNone<ErrandClaimComponent>()
                    .WithSharedComponentFilter(animationRenderMesh)
                .ForEach((int entityInQueryIndex, Entity self, ref AnimationIndexComponent animationIndex, in GrowingThingComponent growingThing) =>
                    {
                        if (growingThing.Grown)
                        {
                            animationIndex.Value = totalFramesInMesh - 1;
                            commandBuffer.AddComponent(entityInQueryIndex, self, new ErrandClaimComponent
                            {
                                Claimed = false
                            });
                        }
                        else
                        {
                            animationIndex.SetPageAsFloatInRange(growingThing.currentGrowth, growingThing.finalGrowthAmount, totalFramesInMesh - 1);
                        }
                    }).ScheduleParallel(Dependency);
            }

            errandAvailabilityCommandSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
