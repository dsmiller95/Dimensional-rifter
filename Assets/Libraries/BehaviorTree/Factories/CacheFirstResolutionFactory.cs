using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "CacheFirstResolution", menuName = "Behaviors/Decorator/CacheFirstResolution", order = 10)]
    public class CacheFirstResolutionFactory : NodeFactory
    {
        public NodeFactory child;

        protected override Node OnCreateNode(GameObject target)
        {
            return new CacheFirstResolution(child.CreateNode(target));
        }
    }
}
