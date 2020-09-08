namespace BehaviorTree.Nodes
{
    public abstract class CompositeNode : Node
    {
        public Node[] children { get; private set; }
        public CompositeNode(Node[] children)
        {
            this.children = children;
        }
    }
}
