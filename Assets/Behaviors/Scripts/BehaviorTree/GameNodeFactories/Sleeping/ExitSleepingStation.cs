using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using Assets.WorldObjects.Members.Building;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "ExitSleepingStation", menuName = "Behaviors/Actions/ExitSleepingStation", order = 10)]
    public class ExitSleepingStation : NodeFactory
    {
        public string sleepStationBlackboard = "SleepingBed";

        protected override Node OnCreateNode(GameObject target)
        {
            return
                new ActionOnComponentLeaf<SleepStation>(
                    sleepStationBlackboard,
                    (station) => station.ExitStation(target) ? NodeStatus.SUCCESS : NodeStatus.FAILURE
                );
        }
    }
}
