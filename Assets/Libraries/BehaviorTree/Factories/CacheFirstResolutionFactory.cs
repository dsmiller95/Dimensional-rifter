using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
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
