using Assets.Scripts.Core;
using BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FloatFromInstantiatorComparison : ComponentMemberLeaf<VariableInstantiator>
    {
        private Func<float, bool> testOnFloatValue;

        private FloatVariable floatVariable;

        public FloatFromInstantiatorComparison(
            GameObject gameObject,
            FloatState floatState,
            Func<float, bool> testOnFloatValue) : base(gameObject)
        {
            this.testOnFloatValue = testOnFloatValue;
            floatVariable = componentValue.GetFloatValue(floatState.IdentifierInInstantiator);
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            return testOnFloatValue(floatVariable.CurrentValue) ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
