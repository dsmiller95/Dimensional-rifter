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

        private ErrandBoard.ErrandClaimingNode sourceNode;

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
            sourceNode = null;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out IErrand errand))
            {
                // only grab an errand if there is none already -- this node must be reset in order to grab another
                return NodeStatus.SUCCESS;
            }
            if(sourceNode != null)
            {
                return EvaluateErrandSourceNode(blackboard);
            }

            sourceNode = errandBoard.AttemptClaimAnyErrandOfType(errandType, target);
            if (sourceNode == null)
            {
                return NodeStatus.FAILURE;
            }
            return EvaluateErrandSourceNode(blackboard);
        }

        private NodeStatus EvaluateErrandSourceNode(Blackboard blackboard)
        {
            var result = sourceNode.Evaluate(blackboard);
            switch (result)
            {
                case NodeStatus.FAILURE:
                    // this will cause this node to try to find another errand on the next evaluate
                    sourceNode = null;
                    return NodeStatus.FAILURE;
                case NodeStatus.SUCCESS:
                    SetErrandToBlackboard(blackboard, sourceNode.resultingErrand);
                    return NodeStatus.SUCCESS;
                case NodeStatus.RUNNING:
                    return NodeStatus.RUNNING;
                default:
                    return NodeStatus.FAILURE;
            }
        }

        private void SetErrandToBlackboard(Blackboard blackboard, IErrand errand)
        {
            blackboard.SetValue(errandPathInBlackboard, errand);
        }
    }
}
