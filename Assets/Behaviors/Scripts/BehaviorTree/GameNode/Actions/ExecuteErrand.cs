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
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out ErrandHandler errand))
            {
                errand.Complete();
            }
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(errandPathInBlackboard, out ErrandHandler errand))
            {
                if (errand.IsComplete)
                {
                    return NodeStatus.SUCCESS;
                }
                var result = errand.errand.Execute(blackboard);
                if (result != NodeStatus.RUNNING)
                {
                    errand.Complete();
                }
                return result;
            }

            return NodeStatus.FAILURE;
        }
    }
}
