using System;
using UnityEngine;

namespace BehaviorTree.Nodes
{
    public class ActionOnTargetComponentLeaf<T> : ComponentMemberLeaf<T>
    {
        private Func<T, NodeStatus> action;

        public ActionOnTargetComponentLeaf(
            GameObject target,
            Func<T, NodeStatus> action) : base(target)
        {
            this.action = action;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            return action(componentValue);
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
