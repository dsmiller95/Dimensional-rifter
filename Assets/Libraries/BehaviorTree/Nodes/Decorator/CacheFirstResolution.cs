namespace BehaviorTree.Nodes
{
    public class CacheFirstResolution : Decorator
    {
        public CacheFirstResolution(BehaviorNode child) : base(child)
        {
        }

        private NodeStatus cachedStatus;

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (cachedStatus == NodeStatus.RUNNING)
            {
                cachedStatus = Child.Evaluate(blackboard);
            }
            return cachedStatus;
        }

        public override void Reset(Blackboard blackboard)
        {
            cachedStatus = NodeStatus.RUNNING;

            base.Reset(blackboard);
        }
    }
}
