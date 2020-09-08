namespace BehaviorTree.Nodes
{
    /// <summary>
    /// Reset the child node only if it fails
    /// </summary>
    public class ResetIfFail : Decorator
    {
        public ResetIfFail(Node child) : base(child)
        {
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            var result = child.Evaluate(blackboard);
            if (result == NodeStatus.FAILURE)
            {
                Reset(blackboard);
            }
            return result;
        }

        public override void Reset(Blackboard blackboard)
        {
            base.Reset(blackboard);
        }
    }
}
