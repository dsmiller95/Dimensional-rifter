namespace BehaviorTree.Nodes
{
    public abstract class CompositeNode : BehaviorNode
    {
        public BehaviorNode[] children { get; private set; }
        public CompositeNode(BehaviorNode[] children)
        {
            this.children = children;
        }
    }
}
