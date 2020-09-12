using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "IsHungryGuard", menuName = "Behaviors/Actions/IsHungryGuard", order = 10)]
    public class IsHungryGuardFactory : NodeFactory
    {
        public float hungerLevel;
        public string calorieBlackboardPath = "calories";

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new ResetIfStatus(NodeStatus.FAILURE, new Sequence(
                new GetCalories(
                    target,
                    calorieBlackboardPath
                ),
                new ComparisonFromBlackboard(
                    calorieBlackboardPath,
                    currentCalories => currentCalories < hungerLevel
                )
            ));
        }
    }
}
