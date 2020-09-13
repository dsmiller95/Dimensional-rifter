using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "FindRandomWalkPath", menuName = "Behaviors/Actions/FindRandomWalkPath", order = 10)]
    [FactoryGraphNode("Leaf/FindRandomWalkPath", "FindRandomWalkPath", 0)]
    public class FindRandomWalkPathFactory : LeafFactory
    {
        public int randomWalkLength;
        public string blackboardPathProperty;
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new SetRandomWalkTarget(
                target,
                blackboardPathProperty,
                randomWalkLength);
        }
    }
}
