using Unity.Entities;
using Unity.Jobs;

namespace ECS_SpriteSheetAnimation
{
    public class AnimationUVCalculatorSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<AnimationRenderMesh>()
                .WithChangeFilter<AnimationIndexComponent>()
                .ForEach((
                    ref AnimationUVComponent animationUV,
                    in AnimationRenderMesh spriteSheetAnimationData,
                    in AnimationIndexComponent index) =>
                {
                    animationUV.Value = spriteSheetAnimationData.GetUVAtIndex(index.Value);
                }).WithoutBurst().Run();
        }
    }
}