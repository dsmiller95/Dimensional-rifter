using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Factories
{
    public abstract class CompositeFactory : NodeFactory
    {
        public NodeFactory[] children;

        public override void SetChildFactories(IEnumerable<NodeFactory> children)
        {
            this.children = children.ToArray();
        }

    }
}
