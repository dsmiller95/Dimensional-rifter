using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers
{
    public abstract class FindTargetPath : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private string pathTargetPropertyInBlackboard;

        public FindTargetPath(
            GameObject gameObject,
            string pathTargetProperty) : base(gameObject)
        {
            pathTargetPropertyInBlackboard = pathTargetProperty;
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            NavigationPath currentPath;
            if (blackboard.TryGetValueOfType(pathTargetPropertyInBlackboard, out currentPath))
            {
                return NodeStatus.SUCCESS;
            }

            var possiblePath = TryGetPath(blackboard);

            if (!possiblePath.HasValue)
            {
                return NodeStatus.FAILURE;
            }
            blackboard.SetValue(pathTargetPropertyInBlackboard, possiblePath.Value);
            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
            blackboard.ClearValue(pathTargetPropertyInBlackboard);
        }

        protected abstract NavigationPath? TryGetPath(Blackboard blackboard);

    }
}
