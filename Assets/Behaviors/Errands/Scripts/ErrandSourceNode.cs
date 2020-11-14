using BehaviorTree.Nodes;

namespace Assets.Behaviors.Errands.Scripts
{
    public interface IErrandSourceNode<out T> where T : IErrand
    {
        T ErrandResult { get; }
        NodeStatus Evaluate(Blackboard blackboard);
    }
    public abstract class ErrandSourceNode<T> : BehaviorNode, IErrandSourceNode<T> where T : IErrand
    {
        public T ErrandResult { get; protected set; }
    }

    public class ImmediateErrandSourceNode<T> : ErrandSourceNode<T> where T : IErrand
    {
        public ImmediateErrandSourceNode(T result)
        {
            ErrandResult = result;
        }
        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            return ErrandResult == null ? NodeStatus.FAILURE : NodeStatus.SUCCESS;
        }
        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
