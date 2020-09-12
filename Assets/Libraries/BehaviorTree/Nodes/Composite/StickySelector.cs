using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Nodes
{
    public class StickySelector : CompositeNode
    {
        private int childIndex = 0;
        public StickySelector(params BehaviorNode[] children) : base(children)
        {
        }
        public StickySelector(IEnumerable<BehaviorNode> children) : this(children.ToArray())
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (childIndex >= children.Length)
            {
                return NodeStatus.FAILURE;
            }
            var status = children[childIndex].Evaluate(blackboard);

            while (status == NodeStatus.FAILURE)
            {
                childIndex++;
                if (childIndex >= children.Length)
                {
                    return NodeStatus.FAILURE;
                }
                status = children[childIndex].Evaluate(blackboard);
            }
            return status;
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
