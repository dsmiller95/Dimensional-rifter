using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "CacheFirstResolution", menuName = "Behaviors/Decorator/CacheFirstResolution", order = 10)]
    [FactoryGraphNode("Decorator/CacheFirstResolution", "CacheFirstResolution", 1)]
    public class CacheFirstResolutionFactory : DecoratorFactory
    {
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new CacheFirstResolution(child.CreateNode(target));
        }
    }
}
