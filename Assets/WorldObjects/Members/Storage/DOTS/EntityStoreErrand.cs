using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.UI.Buttery_Toast;
using Assets.UI.ItemTransferAnimations;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using BehaviorTree.Nodes;
using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    public class EntityStoreErrand : IErrand
    {
        private BehaviorNode ErrandBehaviorTreeRoot;

        public GameObject storingWorker;

        private IErrandCompletionReciever<EntityStoreErrand> notifier;
        private bool BehaviorCompleted = false;

        private StorageSupplyErrandResultComponent errandResult;
        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();

        public EntityStoreErrand(
            StorageSupplyErrandResultComponent storageSupplyErrandResult,
            GameObject actor,
            IErrandCompletionReciever<EntityStoreErrand> notifier)
        {
            errandResult = storageSupplyErrandResult;

            storingWorker = actor;

            this.notifier = notifier;

            SetupBehavior();
        }

        private void SetupBehavior()
        {
            var actualTransferAmount = errandResult.amountToTransfer;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var toastMessage = $"{actualTransferAmount} {Enum.GetName(typeof(Resource), errandResult.resourceTransferType)}";

            var sourceCoordinate = entityManager.GetComponentData<UniversalCoordinatePositionComponent>(errandResult.itemSource).coordinate;
            var sourcePosition = entityManager.GetComponentData<Translation>(errandResult.itemSource);

            var targetCoordinate = entityManager.GetComponentData<UniversalCoordinatePositionComponent>(errandResult.supplyTarget).coordinate;
            var targetPosition = entityManager.GetComponentData<Translation>(errandResult.supplyTarget);

            var actorsInventory = storingWorker.GetComponent<InventoryHoldingController>();

            ErrandBehaviorTreeRoot =
            new Selector(
                new Sequence(
                    new FindPathToCoordinate(
                        storingWorker,
                        sourceCoordinate,
                        "Path",
                        true),
                    new NavigateToTarget(
                        storingWorker,
                        "Path",
                        "target",
                        false),
                    new Sequence(
                        new LabmdaLeaf(black =>
                        {
                            ItemTransferParticleProvider.ShowItemTransferAnimation(sourcePosition, storingWorker);
                            return NodeStatus.SUCCESS;
                        }),
                        new Wait(ItemTransferParticleProvider.Instance.ItemTransferAnimationTime),
                        new LabmdaLeaf(black =>
                        {
                            ToastProvider.ShowToast(
                                toastMessage,
                                sourcePosition
                                );

                            var actionEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                            ClearItemSourceClaim(actionEntityManager);

                            var itemAmount = actionEntityManager.GetComponentData<ItemAmountComponent>(errandResult.itemSource);
                            actualTransferAmount = actorsInventory.GrabUnclaimedItemIntoSelf(errandResult.resourceTransferType, actualTransferAmount);

                            itemAmount.resourceAmount -= actualTransferAmount;
                            actionEntityManager.SetComponentData(errandResult.itemSource, itemAmount);

                            return NodeStatus.SUCCESS;
                        })
                    ),
                    new FindPathToCoordinate(
                        storingWorker,
                        targetCoordinate,
                        "Path",
                        true),
                    new NavigateToTarget(
                        storingWorker,
                        "Path",
                        "target",
                        false),
                    new Sequence(
                        new LabmdaLeaf(black =>
                        {
                            ItemTransferParticleProvider.ShowItemTransferAnimation(storingWorker, targetPosition);
                            return NodeStatus.SUCCESS;
                        }),
                        new Wait(ItemTransferParticleProvider.Instance.ItemTransferAnimationTime),
                        new LabmdaLeaf(black =>
                        {
                            ToastProvider.ShowToast(
                                toastMessage,
                                targetPosition
                                );

                            var actionEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                            ClearStorageClaim(actionEntityManager);

                            actualTransferAmount = actorsInventory.PullUnclaimedItemFromSelf(errandResult.resourceTransferType, actualTransferAmount);
                            var storageAmountBuffer = actionEntityManager.GetBuffer<ItemAmountClaimBufferData>(errandResult.supplyTarget);

                            int resourceIndexInBuffer = -1;
                            for (int i = 0; i < storageAmountBuffer.Length; i++)
                            {
                                var itemAmount = storageAmountBuffer[i];
                                if (itemAmount.Type == errandResult.resourceTransferType)
                                {
                                    resourceIndexInBuffer = i;
                                    break;
                                }
                            }

                            if (resourceIndexInBuffer == -1)
                            {
                                var newbufferItem = new ItemAmountClaimBufferData
                                {
                                    Amount = actualTransferAmount,
                                    Type = errandResult.resourceTransferType,
                                    TotalSubtractionClaims = 0
                                };
                                storageAmountBuffer.Add(newbufferItem);
                            }
                            else
                            {
                                var itemToEdit = storageAmountBuffer[resourceIndexInBuffer];
                                itemToEdit.Amount += actualTransferAmount;
                                storageAmountBuffer[resourceIndexInBuffer] = itemToEdit;
                            }
                            return NodeStatus.SUCCESS;
                        })
                    ),
                    new LabmdaLeaf(blackboard =>
                    {
                        //grabAllocation.Release();
                        //gibAllocation.Release();
                        BehaviorCompleted = true;
                        notifier.ErrandCompleted(this);
                        return NodeStatus.SUCCESS;
                    })
                ),
                new LabmdaLeaf(blackboard =>
                {
                    Debug.Log("storage errand encountered an error. Ensure it has aborted correctly");
                    return NodeStatus.FAILURE;
                })
            );
        }

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }

        private void ClearStorageClaim(EntityManager entityManager)
        {
            var storageComponent = entityManager.GetComponentData<StorageDataComponent>(errandResult.supplyTarget);

            // be sure to de-allocate based on the original subtraction claim, not the modified amount
            storageComponent.TotalAdditionClaims -= errandResult.amountToTransfer;
            entityManager.SetComponentData(errandResult.supplyTarget, storageComponent);
        }
        private void ClearItemSourceClaim(EntityManager entityManager)
        {
            var itemAmount = entityManager.GetComponentData<ItemAmountComponent>(errandResult.itemSource);

            // be sure to de-allocate based on the original subtraction claim, not the modified amount
            itemAmount.TotalAllocatedSubtractions -= errandResult.amountToTransfer;
            entityManager.SetComponentData(errandResult.itemSource, itemAmount);
        }

        private void ClearAllClaims()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            ClearStorageClaim(entityManager);
            ClearItemSourceClaim(entityManager);
        }

        public void OnReset()
        {
            if (!BehaviorCompleted)
            {
                // TODO: can we test that this works?
                ClearAllClaims();
                notifier.ErrandAborted(this);
            }
            ErrandBehaviorTreeRoot = null;
        }
    }
}
