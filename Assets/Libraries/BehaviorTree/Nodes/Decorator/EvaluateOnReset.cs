namespace BehaviorTree.Nodes
{
    /// <summary>
    /// Do nothing, except when reset. On reset will evaluate the child node, throw out the result, and immediately reset the child node
    /// </summary>
    public class EvaluateOnReset : Decorator
    {
        private NodeStatus statusOnEvaluate;
        public EvaluateOnReset(NodeStatus statusOnEvaluate, BehaviorNode child) : base(child)
        {
            this.statusOnEvaluate = statusOnEvaluate;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            return statusOnEvaluate;
        }

        public override void Reset(Blackboard blackboard)
        {
            Child.Evaluate(blackboard);
            base.Reset(blackboard);
        }
    }
}
