namespace Assets.Behaviors.Scripts.BehaviorTree.Nodes
{
    public enum NodeStatus
    {
        FAILURE,
        SUCCESS,
        RUNNING
    }

    public abstract class Node
    {
        public abstract NodeStatus Evaluate(Blackboard blackboard);

        public abstract void Reset(Blackboard blackboard);
    }
}
