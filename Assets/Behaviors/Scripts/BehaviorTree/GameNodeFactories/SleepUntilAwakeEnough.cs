using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "SleepUntilAwakeEnough", menuName = "Behaviors/Actions/SleepUntilAwakeEnough", order = 10)]
    public class SleepUntilAwakeEnough : NodeFactory
    {
        public float restSpeed;
        public FloatState wakefullnessState;

        public float awakefullnessMinimumBeforeEntry;
        public float awakefullnessMinimumRequiredForExit;

        public string sleepingAnimationTrigger = "StartSleeping";
        public float waitTimeForIdleTransition = .5f;
        public string idlingAnimationTrigger = "Idling";

        protected override Node OnCreateNode(GameObject target)
        {
            return
            new Sequence(
                new FloatFromInstantiatorComparison(
                    target,
                    wakefullnessState,
                    wakefullness => wakefullness < awakefullnessMinimumBeforeEntry
                ),
                new AnimationSetTrigger(
                    target,
                    sleepingAnimationTrigger
                ),
                new Selector( // selector with a comparison executes while the comparison is false
                    new FloatFromInstantiatorComparison(
                        target,
                        wakefullnessState,
                        wakefullness => wakefullness > awakefullnessMinimumRequiredForExit
                    ),
                    new Sleep(
                        target,
                        wakefullnessState,
                        restSpeed,
                        awakefullnessMinimumRequiredForExit
                    )
                ),
                new AnimationSetTrigger(
                    target,
                    idlingAnimationTrigger
                ),
                new Wait(waitTimeForIdleTransition)
            );
        }
    }
}
