namespace BehaviorTree.Factories
{
    public abstract class LeafFactory : NodeFactory
    {
        public override int GetValidChildCount()
        {
            return 0;
        }
    }
}
