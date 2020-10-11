using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "IsHungryGuard", menuName = "Behaviors/Actions/IsHungryGuard", order = 10)]
    [FactoryGraphNode("Leaf/IsHungryGuard", "IsHungryGuard", 0)]
    public class IsHungryGuardFactory : LeafFactory
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
