using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace ECS_SpriteSheetAnimation
{
    [UpdateAfter(typeof(AnimationUVCalculatorSystem))]
    public class SpriteSheetRendererSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            Vector4[] uv = new Vector4[1];
            Camera camera = Camera.main;
            Entities.ForEach((in LocalToWorld transformMatrix, in AnimationRenderMesh spriteSheetAnimationData, in AnimationUVComponent uvComponent) =>
            {
                uv[0] = uvComponent.Value;
                materialPropertyBlock.SetVectorArray("_MainTex_UV", uv);

                Graphics.DrawMesh(
                    spriteSheetAnimationData.mesh,
                    transformMatrix.Value,
                    spriteSheetAnimationData.material,
                    0, // Layer
                    camera,
                    0, // Submesh index
                    materialPropertyBlock
                );
            })
                .WithoutBurst()
                .Run();
        }

    }

}