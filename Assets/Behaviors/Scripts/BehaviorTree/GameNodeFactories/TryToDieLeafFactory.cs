using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "TryToDie", menuName = "Behaviors/Actions/TryToDie", order = 10)]
    public class TryToDieLeafFactory : NodeFactory
    {
        public string calorieBlackboardPath = "currentCalories";
        public float calorieDieThreshold = 0f;
        protected override Node OnCreateNode(GameObject target)
        {
            return
            new Sequence(
                new ResetIfFail(new Sequence(
                    new GetCalories(
                        target,
                        calorieBlackboardPath
                    ),
                    new IsBelowThreshold(
                        calorieBlackboardPath,
                        calorieDieThreshold
                    )
                )),
                new Die(target)
            );
        }
    }
}
