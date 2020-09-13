using System.Collections.Generic;

namespace BehaviorTree.Factories
{
    public abstract class LeafFactory : NodeFactory
    {
        public override void SetChildFactories(IEnumerable<NodeFactory> children)
        {
        }
    }
}
