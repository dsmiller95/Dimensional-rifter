using Unity.Entities;

namespace ECS_SpriteSheetAnimation.FlibookComponents
{
    public struct FlipbookAnimatorComponent : IComponentData
    {
        /// <summary>
        /// a number in [0,1) pointing to the current spot in the animation 
        /// </summary>
        public float CurrentAnimationPoint;
        /// <summary>
        /// how quickly to progress through the animation, WRT the total animation size
        /// </summary>
        public float AnimationSpeed;
        public void StepAnimation()
        {
            CurrentAnimationPoint += AnimationSpeed;
            CurrentAnimationPoint %= 1;
        }
    }
}
