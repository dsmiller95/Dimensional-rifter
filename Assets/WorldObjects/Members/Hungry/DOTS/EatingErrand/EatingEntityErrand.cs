using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.DOTS.ErrandClaims;
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
    public class EatingEntityErrand :
        BasicErrand<SpecificResourceErrandResultComponent, EatingEntityErrand>
    {
        public EatingEntityErrand(
            SpecificResourceErrandResultComponent errandResult,
            World entityWorld,
            GameObject actor,
            IErrandCompletionReciever<EatingEntityErrand> completionReciever)
            : base(errandResult, entityWorld, actor, completionReciever)
        {
        }
        protected override BehaviorNode SetupBehavior()
        {
            var manager = entityWorld.EntityManager;
            var targetEntity = errandResult.consumeTarget;
            var targetCoordinates = manager.GetComponentData<UniversalCoordinatePositionComponent>(targetEntity);
            var targetPosition = manager.GetComponentData<Translation>(targetEntity);
            return
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

                    var hungry = actor.GetComponent<Hungry>();
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

        public override void OnErrandFailToComplete()
        {
            ClearConsumeClaim(entityWorld.EntityManager);
        }
    }
}
