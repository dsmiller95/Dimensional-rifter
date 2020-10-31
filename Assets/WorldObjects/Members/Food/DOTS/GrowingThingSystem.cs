using ECS_SpriteSheetAnimation;
using Unity.Entities;


namespace Assets.WorldObjects.Members.Food.DOTS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class GrowingThingSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            // TODO: optimize to run parallel over all AnimationRenderMesh
            // https://github.com/needle-mirror/com.unity.rendering.hybrid/blob/e63fc8f20bba348f886ecd78bd1ff34483adc5b9/Unity.Rendering.Hybrid/RenderMeshSystemV2.cs#L201
            Entities
                .ForEach((ref GrowingThingComponent growingThing, ref AnimationIndexComponent animationIndex, in AnimationRenderMesh renderMesh) =>
            {
                var newGrowth = deltaTime * growingThing.growthPerSecond + growingThing.currentGrowth;
                growingThing.SetGrownAmount(newGrowth);

                if (growingThing.Grown)
                {
                    animationIndex.Value = renderMesh.totalFrames - 1;
                }
                else
                {
                    animationIndex.SetPageAsFloatInRange(growingThing.currentGrowth, growingThing.finalGrowthAmount, renderMesh.totalFrames - 1);
                }
            }).WithoutBurst().Run();
        }

    }
}
