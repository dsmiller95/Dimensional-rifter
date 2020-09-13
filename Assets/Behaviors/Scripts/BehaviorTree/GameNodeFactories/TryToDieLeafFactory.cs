using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "TryToDie", menuName = "Behaviors/Actions/TryToDie", order = 10)]
    [FactoryGraphNode("Leaf/TryToDie", "TryToDie", 0)]
    public class TryToDieLeafFactory : LeafFactory
    {
        public string calorieBlackboardPath = "currentCalories";
        public float calorieDieThreshold = 0f;
        public TileMapMember deadMemberPrefab;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return
            new Sequence(
                new ResetIfStatus(NodeStatus.FAILURE, new Sequence(
                    new GetCalories(
                        target,
                        calorieBlackboardPath
                    ),
                    new ComparisonFromBlackboard(
                        calorieBlackboardPath,
                        currentCalories => currentCalories < calorieDieThreshold
                    )
                )),
                new Die(
                    target,
                    deadMemberPrefab
                )
            );
        }
    }
}
