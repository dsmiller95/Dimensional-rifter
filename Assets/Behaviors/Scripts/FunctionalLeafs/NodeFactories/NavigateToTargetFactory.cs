using Assets.Behaviors.Scripts.BehaviorTree.NodeFactories;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.NodeFactories
{
    [CreateAssetMenu(fileName = "NavigateToTarget", menuName = "Behaviors/Actions/NavigateToTarget", order = 10)]
    public class NavigateToTargetFactory : NodeFactory
    {
        public string blackboardPathProperty;
        public string blackboardTargetProperty;
        public override Node CreateNode(GameObject target)
        {
            return new NavigateToTarget(
                target,
                blackboardPathProperty,
                blackboardTargetProperty);
        }
    }
}
