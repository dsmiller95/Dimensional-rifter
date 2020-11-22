using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [FactoryGraphNode("Decorator/OnReset", "OnReset", 1)]
    public class OnResetFactory : DecoratorFactory
    {
        public NodeStatus ConstantResultOnEvaluate = NodeStatus.SUCCESS;
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new EvaluateOnReset(ConstantResultOnEvaluate, child.CreateNode(target));
        }
    }
}
