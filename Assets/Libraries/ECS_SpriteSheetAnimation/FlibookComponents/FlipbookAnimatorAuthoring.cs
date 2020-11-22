using Unity.Entities;
using UnityEngine;

namespace ECS_SpriteSheetAnimation.FlibookComponents
{
    public class FlipbookAnimatorAuthoring : SpriteSheetAnimatorAuthoringComponent
    {
        [Range(0, 1)]
        public float AnimationSpeed;

        public bool AnimationEnabled;
        [Tooltip("Set to -1 for no index when disabled")]
        public int IndexInSpriteSheetWhenAnimationDisabled;

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            dstManager.AddComponentData(entity, new FlipbookAnimatorComponent
            {
                CurrentAnimationPoint = ((float)startingSpriteIndex) / totalSprites,
                AnimationSpeed = AnimationSpeed
            });
            dstManager.AddComponentData(entity, new FlipbookAnimatorEnabledComponent
            {
                Value = AnimationEnabled
            });
            if (IndexInSpriteSheetWhenAnimationDisabled >= 0)
            {
                dstManager.AddSharedComponentData(entity, new FlipbookWhenDisabledIndexComponent
                {
                    Value = IndexInSpriteSheetWhenAnimationDisabled
                });
                if (!AnimationEnabled)
                {
                    dstManager.SetComponentData(entity, new AnimationIndexComponent
                    {
                        Value = IndexInSpriteSheetWhenAnimationDisabled
                    });
                }
            }
        }
    }
}
