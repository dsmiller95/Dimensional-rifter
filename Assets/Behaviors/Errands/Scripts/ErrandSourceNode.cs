using BehaviorTree.Nodes;
using System;

namespace Assets.Behaviors.Errands.Scripts
{
    /// <summary>
    /// The expectation is that when <see cref="Evaluate(Blackboard)"/> returns <see cref="NodeStatus.SUCCESS"/>, <see cref="ErrandResult"/> will be set
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IErrandSourceNode<out T> where T : IErrand
    {
        T ErrandResult { get; }
        NodeStatus Evaluate(Blackboard blackboard);
    }
    public abstract class ErrandSourceNode<T> : BehaviorNode, IErrandSourceNode<T> where T : IErrand
    {
        public T ErrandResult { get; protected set; }
    }

    public class ErrandFromBlackboardDataNode<T>: ErrandSourceNode<T> where T: IErrand
    {
        private BehaviorNode nodeInternal;
        private Func<Blackboard, T> errandGenerator;
        public ErrandFromBlackboardDataNode(BehaviorNode nodeInternal, Func<Blackboard, T> errandGenerator)
        {
            this.nodeInternal = nodeInternal;
            this.errandGenerator = errandGenerator;
        }
        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if(ErrandResult != null)
            {
                return NodeStatus.SUCCESS;
            }
            var status = nodeInternal.Evaluate(blackboard);
            if(status == NodeStatus.SUCCESS)
            {
                ErrandResult = errandGenerator(blackboard);
            }
            return status;
        }
        public override void Reset(Blackboard blackboard)
        {
            nodeInternal.Reset(blackboard);
        }
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
