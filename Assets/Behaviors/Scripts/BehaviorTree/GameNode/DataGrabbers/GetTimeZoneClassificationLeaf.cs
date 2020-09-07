using Assets.WorldObjects;
using BehaviorTree.Nodes;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Finds a target which can be navigated to, and cache the path to that target if found
    ///     in the navigation object attached to the tile member owned by this object
    /// </summary>
    public class GetTimeZoneClassificaitonLeaf : Leaf
    {
        private string targetIntInBlackboard;
        private GameTime timeProvider;

        public GetTimeZoneClassificaitonLeaf(
            GameTime timeProvider,
            string targetIntInBlackboard) : base()
        {
            this.timeProvider = timeProvider;
            this.targetIntInBlackboard = targetIntInBlackboard;
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            var currentTimeZone = timeProvider.GetTimezone();
            blackboard.SetValue(targetIntInBlackboard, (int)currentTimeZone);
            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
            blackboard.ClearValue(targetIntInBlackboard);
        }
    }
}
