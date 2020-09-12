using Assets.Scripts.Core;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Die
    /// </summary>
    public class Sleep : ComponentMemberLeaf<VariableInstantiator>
    {
        private float restSpeed;
        //private float stopRestingPoint;
        private FloatVariable floatFromInstantiator;

        public Sleep(
            GameObject gameObject,
            FloatState wakefullness,
            float restSpeed,
            float targetRest
            ) : base(gameObject)
        {
            this.restSpeed = restSpeed;
            //this.stopRestingPoint = targetRest;

            floatFromInstantiator = componentValue.GetFloatValue(wakefullness.IdentifierInInstantiator);
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            var currentValue = floatFromInstantiator.CurrentValue;
            //if (currentValue >= stopRestingPoint)
            //{
            //    return NodeStatus.SUCCESS;
            //}

            floatFromInstantiator.SetValue(currentValue + restSpeed * Time.deltaTime);
            return NodeStatus.RUNNING;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
