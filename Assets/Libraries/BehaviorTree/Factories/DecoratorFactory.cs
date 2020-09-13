using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Factories
{
    public abstract class DecoratorFactory : NodeFactory
    {
        public NodeFactory child;
        public override void SetChildFactories(IEnumerable<NodeFactory> children)
        {
            child = children.FirstOrDefault();
        }
    }
}
