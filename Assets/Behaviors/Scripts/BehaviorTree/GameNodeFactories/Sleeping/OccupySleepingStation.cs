using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "OccupySleepingStation", menuName = "Behaviors/Actions/OccupySleepingStation", order = 10)]
    [FactoryGraphNode("Leaf/OccupySleepingStation", "OccupySleepingStation", 0)]
    public class OccupySleepingStation : LeafFactory
    {
        [Header("Find, navigate to, and occupy a sleeping station")]
        public string sleepPathBlackboard = "Path";
        public string sleepStationBlackboard = "SleepingBed";

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return
            new Sequence(
                new FindAndClaimSleepStation(
                    target,
                    sleepPathBlackboard
                ),
                new NavigateToTarget(
                    target,
                    sleepPathBlackboard,
                    sleepStationBlackboard
                ),// todo: will have to handle if someone gets to my station first?
                new ActionOnComponentInBlackboardLeaf<SleepStation>(
                    sleepStationBlackboard,
                    (station) => station.OccupyStation(target) ? NodeStatus.SUCCESS : NodeStatus.FAILURE
                )
            );
        }
    }
}
