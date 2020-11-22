using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Food.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    public class SleepStationOccupyErrand : IErrand
    {
        public static readonly string FOUND_SLEEP_STATION_PATH = "CurrentlySleepingStation";

        public SleepStationOccupyErrandResultComponent errandResult;
        public GameObject sleeper;

        private World entityWorld;
        private EntityCommandBufferSystem commandBufferSystem => entityWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private IErrandCompletionReciever<SleepStationOccupyErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public SleepStationOccupyErrand(
            SleepStationOccupyErrandResultComponent errandResult,
            World entityWorld,
            GameObject sleeper,
            IErrandCompletionReciever<SleepStationOccupyErrand> completionReciever)
        {
            this.entityWorld = entityWorld;
            this.errandResult = errandResult;
            this.sleeper = sleeper;
            this.completionReciever = completionReciever;

            SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        private void SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var targetEntity = errandResult.sleepTarget;
            var targetCoordinates = manager.GetComponentData<UniversalCoordinatePositionComponent>(targetEntity);
            var targetPosition = manager.GetComponentData<Translation>(targetEntity);
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToCoordinate(
                    sleeper,
                    targetCoordinates.Value,
                    "Path",
                    false),
                new NavigateToTarget(
                    sleeper,
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

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }
        public void OnReset()
        {
            if (!BehaviorCompleted)
            {
                ClearSleepErrandClaim(commandBufferSystem.CreateCommandBuffer());
                completionReciever.ErrandAborted(this);
            }
            ErrandBehaviorTreeRoot = null;
        }
    }
}
