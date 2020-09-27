using System;
using UnityEngine;

namespace BehaviorTree.Nodes
{
    public class ActionOnComponentInBlackboardLeaf<T> : Leaf
    {
        private Func<T, NodeStatus> action;
        private string targetObjectProperty;

        public ActionOnComponentInBlackboardLeaf(
            string targetObjectProperty,
            Func<T, NodeStatus> action)
        {
            this.action = action;
            this.targetObjectProperty = targetObjectProperty;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectProperty, out GameObject value))
            {
                var component = value.GetComponent<T>();
                if (component == null)
                {
                    return NodeStatus.FAILURE;
                }
                return action(component);
            }
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
