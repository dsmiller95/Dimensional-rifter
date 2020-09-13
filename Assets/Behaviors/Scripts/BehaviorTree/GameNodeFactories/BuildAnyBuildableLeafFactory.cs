using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "BuildAnyBuildable", menuName = "Behaviors/Actions/BuildAnyBuildable", order = 10)]
    [FactoryGraphNode("Leaf/BuildAnyBuildable", "BuildAnyBuildable", 0)]
    public class BuildAnyBuildableLeafFactory : LeafFactory
    {
        public string blackboardPathProperty = "currentPath";
        public string buildableProperty = "currentNavigationTarget";
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Sequence(
                    new FindBuildableTarget(
                        target,
                        blackboardPathProperty
                    ),
                    new NavigateToTarget(
                        target,
                        blackboardPathProperty,
                        buildableProperty
                    ),
                    new ActionOnComponentLeaf<Buildable>(
                        buildableProperty,
                        buildable => buildable.BuildIfPossible() ? NodeStatus.SUCCESS : NodeStatus.FAILURE
                    )
                );
        }
    }
}
