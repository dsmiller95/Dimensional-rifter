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

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            Object.Destroy(componentValue.gameObject);
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
