namespace BehaviorTree.Factories
{
    public abstract class DecoratorFactory : NodeFactory
    {
        public override int GetValidChildCount()
        {
            return 1;
        }
    }
}
