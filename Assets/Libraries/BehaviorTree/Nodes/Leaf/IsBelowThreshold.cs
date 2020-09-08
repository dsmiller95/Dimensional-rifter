using BehaviorTree.Nodes;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class IsBelowThreshold : Leaf
    {
        private string blackboardProperty;
        private float thresholdValue;

        public IsBelowThreshold(
            string blackboardProperty,
            float thresholdValue)
        {
            this.blackboardProperty = blackboardProperty;
            this.thresholdValue = thresholdValue;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(blackboardProperty, out float comparisonValue) && comparisonValue < thresholdValue)
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
