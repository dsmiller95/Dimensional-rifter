using Assets.WorldObjects.Members.Hungry;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Die
    /// </summary>
    public class Die : ComponentMemberLeaf<Hungry>
    {

        public Die(
            GameObject gameObject
            ) : base(gameObject)
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            Object.Destroy(componentValue.gameObject);
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
