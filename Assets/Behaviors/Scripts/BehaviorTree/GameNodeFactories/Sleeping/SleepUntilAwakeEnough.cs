using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "SleepUntilAwakeEnough", menuName = "Behaviors/Actions/SleepUntilAwakeEnough", order = 10)]
    [FactoryGraphNode("Leaf/SleepUntilAwakeEnough", "SleepUntilAwakeEnough", 0)]
    public class SleepUntilAwakeEnough : LeafFactory
    {
        public float restSpeed;
        public FloatState wakefullnessState;

        public float awakefullnessMinimumRequiredForExit;

        public string sleepingAnimationTrigger = "StartSleeping";
        public float waitTimeForIdleTransition = .5f;
        public string idlingAnimationTrigger = "Idling";

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return
            new Sequence(
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
