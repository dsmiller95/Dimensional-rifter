using Assets.Behaviors.Errands.Scripts;
using BehaviorTree.Nodes;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class ExecuteErrand : Leaf
    {
        private string errandPathInBlackboard;

        public ExecuteErrand(
            string errandPathInBlackboard
            )
        {
            this.errandPathInBlackboard = errandPathInBlackboard;
        }
        public override void Reset(Blackboard blackboard)
        {
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out IErrand errand))
            {
                return errand.Execute(blackboard);
            }

            return NodeStatus.FAILURE;
        }
    }
}
