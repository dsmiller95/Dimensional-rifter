using Unity.Entities;
using Unity.Mathematics;

namespace ECS_SpriteSheetAnimation
{
    public struct AnimationUVComponent : IComponentData
    {
        public float4 Value;
    }
}
