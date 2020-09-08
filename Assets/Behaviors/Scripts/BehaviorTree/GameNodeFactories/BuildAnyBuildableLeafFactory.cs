using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "BuildAnyBuildable", menuName = "Behaviors/Actions/BuildAnyBuildable", order = 10)]
    public class BuildAnyBuildableLeafFactory : NodeFactory
    {
        public string blackboardPathProperty = "currentPath";
        public string buildableProperty = "currentNavigationTarget";
        protected override Node OnCreateNode(GameObject target)
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
