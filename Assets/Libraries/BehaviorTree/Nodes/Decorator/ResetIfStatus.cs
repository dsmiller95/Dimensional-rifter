namespace BehaviorTree.Nodes
{
    /// <summary>
    /// Reset the child node only if it fails
    /// </summary>
    public class ResetIfStatus : Decorator
    {
        private NodeStatus resetStatus;
        public ResetIfStatus(NodeStatus resetStatus, BehaviorNode child) : base(child)
        {
            this.resetStatus = resetStatus;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            var result = Child.Evaluate(blackboard);
            if (result == resetStatus)
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
