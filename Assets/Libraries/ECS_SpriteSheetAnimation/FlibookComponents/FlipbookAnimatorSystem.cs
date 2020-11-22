using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

namespace ECS_SpriteSheetAnimation.FlibookComponents
{
    [UpdateBefore(typeof(AnimationUVCalculatorSystem))]
    public class FlipbookAnimatorSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var allDefaultAnimationIndexes = new List<FlipbookWhenDisabledIndexComponent>();
            EntityManager.GetAllUniqueSharedComponentData(allDefaultAnimationIndexes);
            foreach (var sharedDefaultAnimationIndex in allDefaultAnimationIndexes)
            {
                Entities
                    .WithChangeFilter<FlipbookAnimatorEnabledComponent>()
                    .WithSharedComponentFilter(sharedDefaultAnimationIndex)
                    .ForEach((
                        ref AnimationIndexComponent animationIndex,
                        in FlipbookAnimatorEnabledComponent flipbookEnabled) =>
                    {
                        if (!flipbookEnabled.Value)
                        {
                            animationIndex.Value = sharedDefaultAnimationIndex.Value;
                        }
                    }).ScheduleParallel();
            }

            var allRenderMeshes = new List<AnimationRenderMesh>();
            EntityManager.GetAllUniqueSharedComponentData(allRenderMeshes);
            // TODO: combine all into one big parallel job? keep list of each dependency per shared component
            for (var renderMeshIndex = 0; renderMeshIndex < allRenderMeshes.Count; renderMeshIndex++)
            {
                var animationRenderMesh = allRenderMeshes[renderMeshIndex];

                var totalFramesInMesh = animationRenderMesh.totalFrames;
                Entities
                    .WithSharedComponentFilter(animationRenderMesh)
                    .ForEach((
                        ref FlipbookAnimatorComponent flipbookAnimator,
                        ref AnimationIndexComponent animationIndex,
                        in FlipbookAnimatorEnabledComponent flipbookEnabled) =>
                    {
                        if (!flipbookEnabled.Value)
                        {
                            return;
                        }
                        flipbookAnimator.StepAnimation();
                        animationIndex.SetPageAsFloatInRange(flipbookAnimator.CurrentAnimationPoint, 1f, totalFramesInMesh);
                    }).ScheduleParallel();
            }
        }
    }
}