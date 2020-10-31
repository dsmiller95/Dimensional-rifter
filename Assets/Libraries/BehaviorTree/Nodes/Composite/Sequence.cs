using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Nodes
{
    public class Sequence : CompositeNode
    {
        private int childIndex = 0;

        public Sequence(params BehaviorNode[] children) : base(children)
        {
        }
        public Sequence(IEnumerable<BehaviorNode> children) : this(children.ToArray())
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (childIndex >= children.Length)
            {
                return NodeStatus.SUCCESS;
            }

            var status = children[childIndex].Evaluate(blackboard);

            while (status == NodeStatus.SUCCESS)
            {
                childIndex++;
                if (childIndex >= children.Length)
                {
                    return NodeStatus.SUCCESS;
                }
                status = children[childIndex].Evaluate(blackboard);
            }
            if (status == NodeStatus.FAILURE)
            {
                return NodeStatus.FAILURE;
            }
            return NodeStatus.RUNNING;
        }
        public override void Reset(Blackboard blackboard)
        {
            childIndex = 0;
            foreach (var node in children)
            {
                node.Reset(blackboard);
            }
        }
    }
}
