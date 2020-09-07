using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class ActionOnComponentLeaf<T> : Leaf
    {
        private Func<T, NodeStatus> action;
        private string targetObjectProperty;

        public ActionOnComponentLeaf(
            string targetObjectProperty,
            Func<T, NodeStatus> action)
        {
            this.action = action;
            this.targetObjectProperty = targetObjectProperty;
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectProperty, out GameObject value))
            {
                var component = value.GetComponent<T>();
                if(component == null)
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
