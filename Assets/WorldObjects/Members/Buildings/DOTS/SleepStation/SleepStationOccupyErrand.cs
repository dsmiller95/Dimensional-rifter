using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.DOTS.ErrandClaims;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.DOTSMembers;
using BehaviorTree.Nodes;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    public class SleepStationOccupyErrand :
        BasicErrand<SleepStationOccupyErrandResultComponent, SleepStationOccupyErrand>
    {
        public static readonly string FOUND_SLEEP_STATION_PATH = "CurrentlySleepingStation";

        public SleepStationOccupyErrand(
            SleepStationOccupyErrandResultComponent errandResult,
            World entityWorld,
            GameObject actor,
            IErrandCompletionReciever<SleepStationOccupyErrand> completionReciever)
            : base(errandResult, entityWorld, actor, completionReciever)
        {
        }

        protected override BehaviorNode SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var targetEntity = errandResult.sleepTarget;
            var targetCoordinates = manager.GetComponentData<UniversalCoordinatePositionComponent>(targetEntity);
            var targetPosition = manager.GetComponentData<Translation>(targetEntity);
            return
            new Sequence(
                new FindPathToCoordinate(
                    actor,
                    targetCoordinates.Value,
                    "Path",
                    false),
                new NavigateToTarget(
                    actor,
                    "Path",
                    "target",
                    false),
                new LabmdaLeaf(blackboard =>
                {
                    var localManager = entityWorld.EntityManager;
                    var commandbuffer = commandBufferSystem.CreateCommandBuffer();

                    commandbuffer.SetComponent(targetEntity, new SleepStationOccupiedComponent
                    {
                        Occupied = true
                    });
                    ClearSleepErrandClaim(commandbuffer);

                    ToastProvider.ShowToast(
                        $"zZzZzZzzzZzZzZzZzzZzZ",
                        targetPosition
                        );

                    BehaviorCompleted = true;
                    completionReciever.ErrandCompleted(this);
                    blackboard.SetValue(FOUND_SLEEP_STATION_PATH, targetEntity);

                    return NodeStatus.SUCCESS;
                })
            );
        }

        private bool sleepErrandClaimCleared = false;
        private void ClearSleepErrandClaim(EntityCommandBuffer commandBuffer)
        {
            if (sleepErrandClaimCleared)
            {
                return;
            }
            sleepErrandClaimCleared = true;

            commandBuffer.SetComponent(errandResult.sleepTarget, new ErrandClaimComponent
            {
                Claimed = false
            });
        }

        public override void OnErrandFailToComplete()
        {
            ClearSleepErrandClaim(commandBufferSystem.CreateCommandBuffer());
        }
    }
}
