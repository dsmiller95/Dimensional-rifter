using Assets.WorldObjects.Members.Hungry;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class GetCalories : ComponentMemberLeaf<Hungry>
    {
        private string caloriesTargetProperty;

        public GetCalories(
            GameObject gameObject,
            string caloriesProperty) : base(gameObject)
        {
            caloriesTargetProperty = caloriesProperty;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            blackboard.SetValue(caloriesTargetProperty, componentValue.currentCalories);
            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
            blackboard.ClearValue(caloriesTargetProperty);
        }
    }
}
