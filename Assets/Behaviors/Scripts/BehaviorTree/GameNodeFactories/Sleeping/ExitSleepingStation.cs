using Assets.WorldObjects.Members.Buildings.DOTS.SleepStation;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [FactoryGraphNode("Leaf/ExitSleepingStation", "ExitSleepingStation", 0)]
    public class ExitSleepingStation : LeafFactory
    {
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return
                new LabmdaLeaf(blackboard =>
                {
                    if (!blackboard.TryGetValueOfType(SleepStationOccupyErrand.FOUND_SLEEP_STATION_PATH, out Entity sleepEntity))
                    {
                        return NodeStatus.FAILURE;
                    }
                    var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                    if (!entityManager.Exists(sleepEntity))
                    {
                        return NodeStatus.FAILURE;
                    }
                    var buffer = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>().CreateCommandBuffer();
                    buffer.SetComponent(sleepEntity, new SleepStationOccupiedComponent
                    {
                        Occupied = false
                    });

                    blackboard.ClearValue(SleepStationOccupyErrand.FOUND_SLEEP_STATION_PATH);

                    return NodeStatus.SUCCESS;
                });
        }
    }
}
