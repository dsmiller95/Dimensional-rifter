using Assets.WorldObjects;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// navigate along a <see cref="NavigationPath"/> in pathProperty in the blackboard. When reached,
    ///     put the reached <see cref="GameObject"/> in targetProperty
    /// </summary>
    public class NavigateToTarget : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private string pathProperty;
        private string targetProperty;
        private bool ensureTargetExists;
        public NavigateToTarget(
            GameObject gameObject,
            string pathProperty,
            string targetProperty,
            bool ensureTargetExists = true) : base(gameObject)
        {
            this.pathProperty = pathProperty;
            this.targetProperty = targetProperty;
            this.ensureTargetExists = ensureTargetExists;
        }
        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            NavigationPath currentPath;
            if (!blackboard.TryGetValueOfType(pathProperty, out currentPath)
                || (ensureTargetExists && currentPath.targetMember == null))
            {
                return NodeStatus.FAILURE;
            }

            var navigationResult = componentValue.AttemptAdvanceAlongPath(currentPath);
            switch (navigationResult)
            {
                case NavigationStatus.ARRIVED:
                    blackboard.ClearValue(pathProperty);
                    blackboard.SetValue(targetProperty, currentPath.targetMember?.gameObject);
                    return NodeStatus.SUCCESS;
                case NavigationStatus.INVALID_TARGET:
                    return NodeStatus.FAILURE;
                case NavigationStatus.APPROACHING:
                    return NodeStatus.RUNNING;
                default:
                    return NodeStatus.FAILURE;
            }
        }

        public override void Reset(Blackboard blackboard)
        {
            blackboard.ClearValue(targetProperty);
        }
    }
}
