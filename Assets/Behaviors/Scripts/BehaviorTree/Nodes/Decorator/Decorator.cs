namespace Assets.Behaviors.Scripts.BehaviorTree.Nodes.Decorator
{
    public abstract class Decorator : Node
    {
        protected Node child;
        public Decorator(Node child)
        {
            this.child = child;
        }

        public override void Reset(Blackboard blackboard)
        {
            child.Reset(blackboard);
        }
    }
}
