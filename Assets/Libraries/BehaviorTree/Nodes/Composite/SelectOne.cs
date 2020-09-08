using System.Collections.Generic;
using System.Linq;

namespace BehaviorTree.Nodes
{
    /// <summary>
    /// Selects the child at indexProperty and runs it. if that index fails, this node fails
    ///     if it succeedes, this node succeeds
    /// If the index at indexProperty is missing or out of bounds, fails
    /// </summary>
    public class SelectOne : CompositeNode
    {
        private string indexProperty;
        public SelectOne(string indexProperty, params Node[] children) : base(children)
        {
            this.indexProperty = indexProperty;
        }

        public SelectOne(string indexProperty, IEnumerable<Node> children) : this(indexProperty, children.ToArray())
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(indexProperty, out int index))
            {
                if (index < 0 || index >= children.Length)
                {
                    return NodeStatus.FAILURE;
                }
                return children[index].Evaluate(blackboard);
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
