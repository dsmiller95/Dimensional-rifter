using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Decorator;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.NodeFactories
{
    [CreateAssetMenu(fileName = "CacheFirstResolution", menuName = "Behaviors/Decorator/CacheFirstResolution", order = 10)]
    public class CacheFirstResolutionFactory : NodeFactory
    {
        public NodeFactory child;

        public override Node CreateNode(GameObject target)
        {
            return new CacheFirstResolution(child.CreateNode(target));
        }
    }
}
