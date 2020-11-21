using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.UI.Buttery_Toast;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Storage.DOTS;
using BehaviorTree.Nodes;
using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry.DOTS.EatingErrand
{
    public class EatingEntityErrand : IErrand
    {
        public SpecificResourceErrandResultComponent errandResult;
        public GameObject eater;

        private World entityWorld;
        private EntityCommandBufferSystem commandBufferSystem => entityWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private IErrandCompletionReciever<EatingEntityErrand> completionReciever;
        private bool BehaviorCompleted = false;

        public EatingEntityErrand(
            SpecificResourceErrandResultComponent errandResult,
            World entityWorld,
            GameObject eater,
            IErrandCompletionReciever<EatingEntityErrand> completionReciever)
        {
            this.entityWorld = entityWorld;
            this.errandResult = errandResult;
            this.eater = eater;
            this.completionReciever = completionReciever;

            SetupBehavior();
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        private void SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var targetEntity = errandResult.consumeTarget;
            var targetCoordinates = manager.GetComponentData<UniversalCoordinatePositionComponent>(targetEntity);
            var targetPosition = manager.GetComponentData<Translation>(targetEntity);
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToCoordinate(
                    eater,
                    targetCoordinates.Value,
                    "Path",
                    true),
                new NavigateToTarget(
                    eater,
                    "Path",
                    "target",
                    false),
                new LabmdaLeaf(blackboard =>
                {
                    // TODO: eating animation
                    return NodeStatus.SUCCESS;
                }),
                new Wait(1),
                new LabmdaLeaf(blackboard =>
                {
                    var localManager = entityWorld.EntityManager;
                    var commandbuffer = commandBufferSystem.CreateCommandBuffer();
                    var growingData = localManager.GetBuffer<ItemAmountClaimBufferData>(targetEntity);
                    var resourceIndex = growingData.IndexOfType(errandResult.resourceType);
                    if (resourceIndex == -1)
                    {
                        Debug.LogError("[ERRANDS][EAT] Resource to eat not found at target entity");
                        return NodeStatus.FAILURE;
                    }
                    var resourceAmount = growingData[resourceIndex];
                    resourceAmount.Amount -= errandResult.amountToConsume;
                    growingData[resourceIndex] = resourceAmount;
                    ClearConsumeClaim(localManager);

                    var hungry = eater.GetComponent<Hungry>();
                    hungry.EatAmount(errandResult.resourceType, errandResult.amountToConsume);

                    ToastProvider.ShowToast(
                        $"Eating {errandResult.amountToConsume:F1} {Enum.GetName(typeof(Resource), errandResult.resourceType)}",
                        targetPosition
                        );

                    BehaviorCompleted = true;
                    completionReciever.ErrandCompleted(this);
                    return NodeStatus.SUCCESS;
                })
            );
        }

        private bool itemSourceClaimCleared = false;
        private void ClearConsumeClaim(EntityManager entityManager)
        {
            if (itemSourceClaimCleared)
            {
                return;
            }
            itemSourceClaimCleared = true;
            if (!entityManager.Exists(errandResult.consumeTarget))
            {
                // The errand could have aborted after picking up the item source,
                // if it was a loose item the entity may not exist anymore
                return;
            }
            var itemAmountBuffer = entityManager.GetBuffer<ItemAmountClaimBufferData>(errandResult.consumeTarget);
            var itemIndex = itemAmountBuffer.IndexOfType(errandResult.resourceType);
            if (itemIndex < 0)
            {
                Debug.LogError("[ERRANDS][EAT] Item to grab not found in the item source");
                return;
            }

            var itemAmount = itemAmountBuffer[itemIndex];
            // be sure to de-allocate based on the original subtraction claim, not the modified amount
            itemAmount.TotalSubtractionClaims -= errandResult.amountToConsume;
            itemAmountBuffer[itemIndex] = itemAmount;
        }


        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }
        public void OnReset()
        {
            if (!BehaviorCompleted)
            {
                ClearConsumeClaim(entityWorld.EntityManager);
                completionReciever.ErrandAborted(this);
            }
            ErrandBehaviorTreeRoot = null;
        }
    }
}
