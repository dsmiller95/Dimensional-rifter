using BehaviorTree.Nodes;
using System;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class ComparisonFromBlackboard : Leaf
    {
        private string blackboardProperty;
        Func<float, bool> comparison;

        public ComparisonFromBlackboard(
            string blackboardProperty,
            Func<float, bool> comparison)
        {
            this.blackboardProperty = blackboardProperty;
            this.comparison = comparison;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(blackboardProperty, out float comparisonValue) && comparison(comparisonValue))
            {
                return NodeStatus.SUCCESS;
            }
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
