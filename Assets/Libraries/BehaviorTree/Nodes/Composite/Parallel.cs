using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Nodes
{
    public class Parallel : CompositeNode
    {
        public Parallel(params Node[] children) : base(children)
        {
        }
        public Parallel(IEnumerable<Node> children) : this(children.ToArray())
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (children.Length <= 0)
            {
                return NodeStatus.FAILURE;
            }
            foreach (var node in children)
            {
                var status = node.Evaluate(blackboard);
                if (status != NodeStatus.RUNNING)
                {
                    return status;
                }
            }
            return NodeStatus.RUNNING;
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
