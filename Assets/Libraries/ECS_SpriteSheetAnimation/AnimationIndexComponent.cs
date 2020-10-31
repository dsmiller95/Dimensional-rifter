using Unity.Entities;
using Unity.Mathematics;

namespace ECS_SpriteSheetAnimation
{
    public struct AnimationIndexComponent : IComponentData
    {
        public int Value;

        public void SetPageAsFloatInRange(float value, float range, int maxIndex)
        {
            var floatPerValueInRange = range / maxIndex;
            var index = (int)math.floor(value / floatPerValueInRange);
            Value = index;
        }
    }
}
