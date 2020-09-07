using Assets.Behaviors.Scripts.BehaviorTree.NodeFactories;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers;
using Assets.Behaviors.Scripts.FunctionalStates;
using Assets.WorldObjects.Members.Building;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.NodeFactories
{
    [CreateAssetMenu(fileName = "BuildAnyBuildable", menuName = "Behaviors/Actions/BuildAnyBuildable", order = 10)]
    public class BuildAnyBuildableLeafFactory : NodeFactory
    {
        public string blackboardPathProperty = "currentPath";
        public string buildableProperty = "currentNavigationTarget";
        public override Node CreateNode(GameObject target)
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
