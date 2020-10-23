using Assets.WorldObjects;
using Assets.WorldObjects.Members;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Die
    /// </summary>
    public class Die : Leaf
    {
        private TileMapMember memberToReplaceWith;
        private GameObject gameObject;
        public Die(
            GameObject gameObject,
            TileMapMember memberToReplaceWith
            )
        {
            this.memberToReplaceWith = memberToReplaceWith;
            this.gameObject = gameObject;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            SelfTileMemberReplacer.ReplaceMember(gameObject, memberToReplaceWith);
            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
