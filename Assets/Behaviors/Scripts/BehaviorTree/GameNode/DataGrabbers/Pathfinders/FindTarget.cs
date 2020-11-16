using Assets.WorldObjects;
using BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Finds a target which can be navigated to, and cache the path to that target if found
    ///     in the navigation object attached to the tile member owned by this object
    /// </summary>
    [Obsolete("Use Entities")]
    public class FindTarget : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private Func<TileMapMember, object, bool> targetFilter;
        private string blackboardSelector;

        private string pathTargetPropertyInBlackboard;

        public FindTarget(
            GameObject gameObject,
            Func<TileMapMember, bool> targetFilter,
            string pathTargetProperty) : this(
                gameObject,
                null,
                (member, obj) => targetFilter(member),
                pathTargetProperty)
        {
        }

        public FindTarget(
            GameObject gameObject,
            string blackboardSelector,
            Func<TileMapMember, object, bool> targetFilter,
            string pathTargetProperty) : base(gameObject)
        {
            this.targetFilter = targetFilter;
            pathTargetPropertyInBlackboard = pathTargetProperty;
            this.blackboardSelector = blackboardSelector;
        }


        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            NavigationPath currentPath;
            if (blackboard.TryGetValueOfType(pathTargetPropertyInBlackboard, out currentPath))
            {
                return NodeStatus.SUCCESS;
            }
            object blackboardValue = null;
            if (blackboardSelector != null)
            {
                blackboard.TryGetValue(blackboardSelector, out blackboardValue);
            }
            var possiblePath = componentValue
                .GetClosestOfTypeWithPath(member => targetFilter(member, blackboardValue));

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
    }
}
