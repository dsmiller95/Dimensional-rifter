using Unity.Entities;

namespace ECS_SpriteSheetAnimation.FlibookComponents
{
    /// <summary>
    /// TODO: Consider making this a flag component
    /// </summary>
    public struct FlipbookAnimatorEnabledComponent : IComponentData
    {
        public bool Value;
    }
}
