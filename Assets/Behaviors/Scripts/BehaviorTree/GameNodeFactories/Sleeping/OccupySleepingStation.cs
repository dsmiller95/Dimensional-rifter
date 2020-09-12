using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "OccupySleepingStation", menuName = "Behaviors/Actions/OccupySleepingStation", order = 10)]
    public class OccupySleepingStation : NodeFactory
    {
        [Header("Find, navigate to, and occupy a sleeping station")]
        public string sleepPathBlackboard = "Path";
        public string sleepStationBlackboard = "SleepingBed";

        protected override Node OnCreateNode(GameObject target)
        {
            return
            new Sequence(
                new FindSleepStation(
                    target,
                    sleepPathBlackboard
                ),
                new NavigateToTarget(
                    target,
                    sleepPathBlackboard,
                    sleepStationBlackboard
                ),// todo: will have to handle if someone gets to my station first?
                new ActionOnComponentLeaf<SleepStation>(
                    sleepStationBlackboard,
                    (station) => station.OccupyStation(target) ? NodeStatus.SUCCESS : NodeStatus.FAILURE
                )
            );
        }
    }
}
