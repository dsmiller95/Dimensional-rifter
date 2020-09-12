namespace BehaviorTree.Nodes
{
    public abstract class Decorator : BehaviorNode
    {
        public BehaviorNode Child { get; private set; }
        public Decorator(BehaviorNode child)
        {
            Child = child;
        }

        public override void Reset(Blackboard blackboard)
        {
            Child.Reset(blackboard);
        }
    }
}
