using Assets.WorldObjects.Members.Building;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "ExitSleepingStation", menuName = "Behaviors/Actions/ExitSleepingStation", order = 10)]
    public class ExitSleepingStation : LeafFactory
    {
        public string sleepStationBlackboard = "SleepingBed";

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return
                new ActionOnComponentLeaf<SleepStation>(
                    sleepStationBlackboard,
                    (station) => station.ExitStation(target) ? NodeStatus.SUCCESS : NodeStatus.FAILURE
                );
        }
    }
}
