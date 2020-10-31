using ECS_SpriteSheetAnimation;
using Unity.Entities;
using UnityEngine;

namespace Assets.Libraries.ECS_SpriteSheetAnimation
{
    public class SpriteSheetAnimatorAuthoringComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Mesh quadMesh;
        public Material spriteSheetMaterial;

        public Vector2Int SpriteSheetSize;
        public int totalSprites;
        public int startingSpriteIndex;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            AnimationRenderMesh renderMesh = new AnimationRenderMesh
            {
                mesh = quadMesh,
                material = spriteSheetMaterial,
                frameCountX = SpriteSheetSize.x,
                frameCountY = SpriteSheetSize.y,
                totalFrames = totalSprites == 0 ? SpriteSheetSize.x * SpriteSheetSize.y : totalSprites
            };
            dstManager.AddSharedComponentData(entity, renderMesh);

            dstManager.AddComponentData(entity, new AnimationIndexComponent
            {
                Value = startingSpriteIndex
            });


            dstManager.AddComponentData(entity, new AnimationUVComponent());
        }
    }
}
