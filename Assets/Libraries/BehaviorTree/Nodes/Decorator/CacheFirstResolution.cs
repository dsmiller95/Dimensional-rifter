namespace BehaviorTree.Nodes
{
    public class CacheFirstResolution : Decorator
    {
        public CacheFirstResolution(Node child) : base(child)
        {
        }

        private NodeStatus cachedStatus;

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            if (cachedStatus != NodeStatus.RUNNING)
            {
                return cachedStatus;
            }
            return child.Evaluate(blackboard);
        }

        public override void Reset(Blackboard blackboard)
        {
            cachedStatus = NodeStatus.RUNNING;

            base.Reset(blackboard);
        }
    }
}
