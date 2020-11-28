using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using BehaviorTree.Nodes;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.DeconstructErrand
{
    public class DeconstructErrand : IErrand
    {
        public DeconstructErrandResultComponent errandResult;
        public GameObject actor;

        private World entityWorld;
        private EntityCommandBufferSystem commandBufferSystem => entityWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private LooseItemSpawnSystem ItemSpawnSystem => entityWorld.GetOrCreateSystem<LooseItemSpawnSystem>();
        private IErrandCompletionReciever<DeconstructErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public DeconstructErrand(
            DeconstructErrandResultComponent errandResult,
            World entityWorld,
            GameObject actor,
            IErrandCompletionReciever<DeconstructErrand> completionReciever)
        {
            this.entityWorld = entityWorld;
            this.errandResult = errandResult;
            this.actor = actor;
            this.completionReciever = completionReciever;

            SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        private void SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var targetEntity = errandResult.deconstructTarget;
            var targetCoordinates = manager.GetComponentData<UniversalCoordinatePositionComponent>(targetEntity);
            var targetPosition = manager.GetComponentData<Translation>(targetEntity);
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToCoordinate(
                    actor,
                    targetCoordinates.Value,
                    "Path",
                    true),
                new NavigateToTarget(
                    actor,
                    "Path",
                    "target",
                    false),
                new LabmdaLeaf(blackboard =>
                {
                    var localManager = entityWorld.EntityManager;
                    var commandbuffer = commandBufferSystem.CreateCommandBuffer();
                    var spawnSystem = ItemSpawnSystem;

                    if (localManager.HasComponent<ItemAmountClaimBufferData>(targetEntity))
                    {
                        var buildingInventory = localManager.GetBuffer<ItemAmountClaimBufferData>(targetEntity);
                        spawnSystem.SpawnAllAsLooseItem(targetCoordinates.Value, buildingInventory, commandbuffer);
                    }

                    var buildingParentData = localManager.GetComponentData<BuildingParentComponent>(targetEntity);
                    var buildingMaterials = localManager.GetBuffer<ItemAmountClaimBufferData>(buildingParentData.buildingEntity);
                    spawnSystem.SpawnAllAsLooseItem(targetCoordinates.Value, buildingMaterials, commandbuffer);

                    commandbuffer.DestroyEntity(targetEntity);
                    commandbuffer.DestroyEntity(buildingParentData.buildingEntity);
                    errandClaimCleared = true;

                    ToastProvider.ShowToast(
                        $"Deconstruct goes brbrrbrrrrr",
                        targetPosition
                        );

                    BehaviorCompleted = true;
                    completionReciever.ErrandCompleted(this);
                    return NodeStatus.SUCCESS;
                })
            );
        }

        private bool errandClaimCleared = false;
        private void ClearErrandClaim(EntityCommandBuffer commandBuffer, EntityManager manager)
        {
            if (errandClaimCleared || manager.Exists(errandResult.deconstructTarget))
            {
                return;
            }
            errandClaimCleared = true;
            commandBuffer.SetComponent(errandResult.deconstructTarget, new DeconstructBuildingClaimComponent
            {
                DeconstructClaimed = false
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
                ClearErrandClaim(commandBufferSystem.CreateCommandBuffer(), entityWorld.EntityManager);
                completionReciever.ErrandAborted(this);
            }
            ErrandBehaviorTreeRoot = null;
        }
    }
}
