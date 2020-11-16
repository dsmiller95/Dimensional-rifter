using Assets.WorldObjects;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    [Obsolete("Use Entities")]
    public class FindAndClaimSleepStation : FindTargetPath
    {
        public FindAndClaimSleepStation(
            GameObject gameObject,
            string pathTargetProperty) : base(gameObject, pathTargetProperty)
        {
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            var path = componentValue
                .GetClosestOfTypeWithPath(SleepStationFilter, false);

            if (path.HasValue)
            {
                var targetSource = path.Value.targetMember.GetComponent<SleepStation>();
                if (!targetSource.ClaimStation(componentValue.gameObject))
                {
                    return null;
                }
            }
            return path;
        }

        private bool SleepStationFilter(TileMapMember member)
        {
            var sleepSpot = member.GetComponent<SleepStation>();
            return sleepSpot != null && sleepSpot.CanBeClaimed();
        }
        public override void Reset(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(pathTargetPropertyInBlackboard, out NavigationPath path))
            {
                var target = path.targetMember?.GetComponent<SleepStation>();
                target.ReleaseStationClaim(componentValue.gameObject);
            }
            base.Reset(blackboard);
        }
    }
}
