using Assets.Behaviors.Errands.Scripts;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class ClaimErrandOfType : Leaf
    {
        private GameObject target;
        private string errandPathInBlackboard;
        private ErrandBoard errandBoard;
        private ErrandType errandType;
        public ClaimErrandOfType(
            GameObject target,
            ErrandBoard errandBoard,
            ErrandType errandType,
            string errandPathInBlackboard
            )
        {
            this.target = target;
            this.errandBoard = errandBoard;
            this.errandType = errandType;
            this.errandPathInBlackboard = errandPathInBlackboard;
        }
        public override void Reset(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out IErrand errand))
            {
                errand.OnReset();
                blackboard.ClearValue(errandPathInBlackboard);
            }
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out IErrand errand))
            {
                // only grab an errand if there is none already -- this node must be reset in order to grab another
                return NodeStatus.SUCCESS;
            }

            var newErrand = errandBoard.AttemptClaimAnyErrandOfType(errandType, target);
            if (newErrand == null)
            {
                return NodeStatus.FAILURE;
            }

            blackboard.SetValue(errandPathInBlackboard, newErrand);
            return NodeStatus.SUCCESS;
        }
    }
}
