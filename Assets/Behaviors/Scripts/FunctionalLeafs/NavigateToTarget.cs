using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs
{
    /// <summary>
    /// navigate along a <see cref="NavigationPath"/> in pathProperty in the blackboard. When reached,
    ///     put the reached <see cref="GameObject"/> in targetProperty
    /// </summary>
    public class NavigateToTarget : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private string pathProperty;
        private string targetProperty;
        public NavigateToTarget(
            GameObject gameObject,
            string pathProperty,
            string targetProperty) : base(gameObject)
        {
            this.pathProperty = pathProperty;
            this.targetProperty = targetProperty;
        }
        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            NavigationPath currentPath;
            if (!blackboard.TryGetValueOfType(pathProperty, out currentPath))
            {
                return NodeStatus.FAILURE;
            }

            var navigationResult = componentValue.AttemptAdvanceAlongPath(currentPath);
            switch (navigationResult)
            {
                case NavigationStatus.ARRIVED:
                    blackboard.ClearValue(pathProperty);
                    blackboard.SetValue(targetProperty, currentPath.targetMember.gameObject);
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
