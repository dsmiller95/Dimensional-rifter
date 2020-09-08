namespace BehaviorTree.Nodes
{
    public class CacheFirstResolution : Decorator
    {
        public CacheFirstResolution(Node child) : base(child)
        {
        }

        private NodeStatus cachedStatus;

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (cachedStatus != NodeStatus.RUNNING)
            {
                return cachedStatus;
            }
            return Child.Evaluate(blackboard);
        }

        public override void Reset(Blackboard blackboard)
        {
            cachedStatus = NodeStatus.RUNNING;

            base.Reset(blackboard);
        }
    }
}
