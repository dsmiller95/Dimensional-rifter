using Assets.Behaviors.Errands.Scripts;
using Assets.UI.Priorities;
using Assets.WorldObjects.Members.Hungry;
using BehaviorTree.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class ClaimHighestPriorityErrand : ComponentMemberLeaf<WorkerController>
    {
        private string errandPathInBlackboard;
        private ErrandBoard errandBoard;
        private PrioritySetToErrandConfiguration prioritySetToErrands;

        private ErrandBoard.ErrandClaimingNode[] ClaimingNodes;

        public ClaimHighestPriorityErrand(
            GameObject target,
            ErrandBoard errandBoard,
            PrioritySetToErrandConfiguration prioritySetToErrands,
            string errandPathInBlackboard
            ) : base(target)
        {
            this.errandBoard = errandBoard;
            this.prioritySetToErrands = prioritySetToErrands;
            this.errandPathInBlackboard = errandPathInBlackboard;
        }
        public override void Reset(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out IErrand errand))
            {
                errand.OnReset();
                blackboard.ClearValue(errandPathInBlackboard);
            }
            ClaimingNodes = null;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out IErrand errand))
            {
                // only grab an errand if there is none already -- this node must be reset in order to grab another
                return NodeStatus.SUCCESS;
            }

            if(ClaimingNodes != null)
            {
                return SelectThroughErrandNodes(blackboard);
            }

            if (!GetClaimingErrandNodes())
            {
                return NodeStatus.FAILURE;
            }
            return SelectThroughErrandNodes(blackboard);
        }

        private bool GetClaimingErrandNodes()
        {
            var priorities = componentValue.myPriorities.priorities;
            var errandTypes = prioritySetToErrands.errandTypesToSetPrioritiesFor;
            if (priorities.Length != errandTypes.Length)
            {
                return false;
            }
            var ErrandsByPriority = new List<(ErrandType, int)>(priorities.Length);

            for (int i = 0; i < priorities.Length; i++)
            {
                ErrandsByPriority.Add((errandTypes[i], priorities[i]));
            }
            ErrandsByPriority.Sort((a, b) => b.Item2 - a.Item2);

            ClaimingNodes = ErrandsByPriority
                .Select(x => errandBoard.AttemptClaimAnyErrandOfType(x.Item1, componentValue.gameObject))
                .ToArray();
            return true;
        }

        private NodeStatus SelectThroughErrandNodes(Blackboard blackboard)
        {
            foreach (var node in ClaimingNodes)
            {
                var nodeResult = node.Evaluate(blackboard);
                switch (nodeResult)
                {
                    case NodeStatus.SUCCESS:
                        SetErrandToBlackboard(blackboard, node.resultingErrand);
                        return NodeStatus.SUCCESS;
                    case NodeStatus.RUNNING:
                        return NodeStatus.RUNNING;
                    case NodeStatus.FAILURE:
                        break;
                    default:
                        break;
                }
            }
            return NodeStatus.FAILURE;
        }

        private void SetErrandToBlackboard(Blackboard blackboard, IErrand errand)
        {
            blackboard.SetValue(errandPathInBlackboard, errand);
        }
    }
}
