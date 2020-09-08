using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Nodes
{
    public class Selector : CompositeNode
    {
        public Selector(params Node[] children) : base(children)
        {
        }
        public Selector(IEnumerable<Node> children) : this(children.ToArray())
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
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
