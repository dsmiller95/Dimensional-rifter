namespace BehaviorTree.Nodes
{
    public abstract class Decorator : Node
    {
        public Node Child { get; private set; }
        public Decorator(Node child)
        {
            Child = child;
        }

        public override void Reset(Blackboard blackboard)
        {
            Child.Reset(blackboard);
        }
    }
}
