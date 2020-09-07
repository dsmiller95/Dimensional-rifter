using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
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
