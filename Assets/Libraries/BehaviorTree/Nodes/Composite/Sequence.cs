using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Nodes
{
    public class Sequence : CompositeNode
    {
        private Node[] children;
        private int childIndex = 0;

        public Sequence(params Node[] children): base(children)
        {
            this.children = children;
        }
        public Sequence(IEnumerable<Node> children) : this(children.ToArray())
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
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
