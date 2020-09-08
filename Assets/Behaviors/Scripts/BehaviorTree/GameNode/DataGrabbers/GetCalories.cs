using Assets.WorldObjects;
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

        public override NodeStatus Evaluate(Blackboard blackboard)
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
