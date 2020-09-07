using System.Collections.Generic;
using System.Linq;

namespace Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite
{
    public class Selector : CompositeNode
    {
        private Node[] children;
        public Selector(params Node[] children)
        {
            this.children = children;
        }
        public Selector(IEnumerable<Node> children) : this(children.ToArray())
        {
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            foreach (var node in children)
            {
                switch (node.Evaluate(blackboard))
                {
                    case NodeStatus.FAILURE:
                        continue;
                    case NodeStatus.SUCCESS:
                        return NodeStatus.SUCCESS;
                    case NodeStatus.RUNNING:
                        return NodeStatus.RUNNING;
                    default:
                        return NodeStatus.FAILURE;
                }
            }
            return NodeStatus.FAILURE;
        }
        public override void Reset(Blackboard blackboard)
        {
            foreach (var node in children)
            {
                node.Reset(blackboard);
            }
        }
    }
}
