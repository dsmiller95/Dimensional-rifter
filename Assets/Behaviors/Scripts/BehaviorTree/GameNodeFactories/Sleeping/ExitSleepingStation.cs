using Assets.WorldObjects.Members.Building;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "ExitSleepingStation", menuName = "Behaviors/Actions/ExitSleepingStation", order = 10)]
    [FactoryGraphNode("Leaf/ExitSleepingStation", "ExitSleepingStation", 0)]
    public class ExitSleepingStation : LeafFactory
    {
        public string sleepStationBlackboard = "SleepingBed";

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return
                new ActionOnComponentInBlackboardLeaf<SleepStation>(
                    sleepStationBlackboard,
                    (station) => station.ExitStation(target) ? NodeStatus.SUCCESS : NodeStatus.FAILURE
                );
        }
    }
}
