using System;

namespace BehaviorTree.Nodes
{
    public class LabmdaLeaf : Leaf
    {
        private Func<Blackboard, NodeStatus> action;

        public LabmdaLeaf(
            Func<Blackboard, NodeStatus> action)
        {
            this.action = action;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            return action(blackboard);
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
