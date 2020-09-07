using Assets.Behaviors.Scripts.BehaviorTree.NodeFactories;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.NodeFactories
{
    [CreateAssetMenu(fileName = "FindTargetByName", menuName = "Behaviors/Actions/FindTargetByName", order = 10)]
    public class FindTargetByNameLeafFactory : NodeFactory
    {
        public string targetGameObjectNamePart;
        public string blackboardPathProperty;
        public override Node CreateNode(GameObject target)
        {
            return new FindTarget(
                target,
                member => member.name.ToLower().Contains(targetGameObjectNamePart),
                blackboardPathProperty);
        }
    }
}
