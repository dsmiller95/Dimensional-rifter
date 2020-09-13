using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "CacheFirstResolution", menuName = "Behaviors/Decorator/CacheFirstResolution", order = 10)]
    public class CacheFirstResolutionFactory : DecoratorFactory
    {
        public NodeFactory child;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new CacheFirstResolution(child.CreateNode(target));
        }
    }
}
