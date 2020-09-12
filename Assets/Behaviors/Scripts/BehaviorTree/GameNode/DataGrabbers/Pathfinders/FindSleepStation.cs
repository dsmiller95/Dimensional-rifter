using Assets.WorldObjects;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    public class FindSleepStation : FindTargetPath
    {
        public FindSleepStation(
            GameObject gameObject,
            string pathTargetProperty) : base(gameObject, pathTargetProperty)
        {
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            return componentValue
                .GetClosestOfTypeWithPath(SleepStationFilter, false);
        }

        private bool SleepStationFilter(TileMapMember member)
        {
            var sleepSpot = member.GetComponent<SleepStation>();
            return sleepSpot != null && sleepSpot.CanBeOccupied();
        }
    }
}
