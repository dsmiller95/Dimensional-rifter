using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.UI.Buttery_Toast;
using Assets.UI.ItemTransferAnimations;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Buildings.DOTS;
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
        EntityCommandBufferSystem commandbufferSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        private LooseItemSpawnSystem itemSpawnSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<LooseItemSpawnSystem>();

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

            var sourceCoordinate = entityManager.GetComponentData<UniversalCoordinatePositionComponent>(errandResult.itemSource).Value;
            var sourcePosition = entityManager.GetComponentData<Translation>(errandResult.itemSource);

            var targetObject = errandResult.supplyTarget;
            if (entityManager.HasComponent<BuildingChildComponent>(targetObject))
            {
                targetObject = entityManager.GetComponentData<BuildingChildComponent>(targetObject).controllerComponent;
            }
            var targetCoordinate = entityManager.GetComponentData<UniversalCoordinatePositionComponent>(targetObject).Value;
            var targetPosition = entityManager.GetComponentData<Translation>(targetObject);

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

                            var itemAmountBuffer = actionEntityManager.GetBuffer<ItemAmountClaimBufferData>(errandResult.itemSource);
                            var itemIndex = itemAmountBuffer.IndexOfType(errandResult.resourceTransferType);
                            if (itemIndex < 0)
                            {
                                Debug.LogError("Item to grab not found in the item source");
                                return NodeStatus.FAILURE;
                            }
                            actualTransferAmount = actorsInventory.GrabUnclaimedItemIntoSelf(errandResult.resourceTransferType, actualTransferAmount);

                            var itemAmount = itemAmountBuffer[itemIndex];
                            itemAmount.Amount -= actualTransferAmount;
                            itemAmountBuffer[itemIndex] = itemAmount;

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
                    return NodeStatus.FAILURE;
                })
            );
        }

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }

        private bool storageClaimCleared = false;
        private void ClearStorageClaim(EntityManager entityManager)
        {
            if (storageClaimCleared)
            {
                return;
            }
            storageClaimCleared = true;
            if (!entityManager.Exists(errandResult.supplyTarget))
            {
                // The errand could have aborted because the supply target no longer exists
                return;
            }
            var storageComponent = entityManager.GetComponentData<ItemAmountsDataComponent>(errandResult.supplyTarget);

            // be sure to de-allocate based on the original subtraction claim, not the modified amount
            storageComponent.TotalAdditionClaims -= errandResult.amountToTransfer;
            entityManager.SetComponentData(errandResult.supplyTarget, storageComponent);
        }
        private bool itemSourceClaimCleared = false;
        private void ClearItemSourceClaim(EntityManager entityManager)
        {
            if (itemSourceClaimCleared)
            {
                return;
            }
            itemSourceClaimCleared = true;
            if (!entityManager.Exists(errandResult.itemSource))
            {
                // The errand could have aborted after picking up the item source,
                // if it was a loose item the entity may not exist anymore
                return;
            }
            var itemAmountBuffer = entityManager.GetBuffer<ItemAmountClaimBufferData>(errandResult.itemSource);
            //TODO: finding this index multiple times. reduce calls?
            var itemIndex = itemAmountBuffer.IndexOfType(errandResult.resourceTransferType);
            if (itemIndex < 0)
            {
                Debug.LogError("[ERRANDS][STORAGE]Item to grab not found in the item source");
                return;
            }

            var itemAmount = itemAmountBuffer[itemIndex];

            // be sure to de-allocate based on the original subtraction claim, not the modified amount
            itemAmount.TotalSubtractionClaims -= errandResult.amountToTransfer;
            itemAmountBuffer[itemIndex] = itemAmount;
        }

        private void DropAllItems()
        {
            var tileMem = storingWorker.GetComponent<TileMapNavigationMember>();
            var actorPos = tileMem.CoordinatePosition;

            var actorsInventory = storingWorker.GetComponent<InventoryHoldingController>();
            var allItems = actorsInventory.DrainAll();
            var buffer = commandbufferSystem.CreateCommandBuffer();
            foreach (var item in allItems)
            {
                if (item.Value <= 1e-5)
                    continue;
                itemSpawnSystem.SpawnLooseItem(
                    actorPos,
                    item.Key,
                    item.Value,
                    buffer);
            }
        }

        private void ClearAllClaims()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            ClearStorageClaim(entityManager);
            ClearItemSourceClaim(entityManager);
            DropAllItems();
        }

        public void OnReset()
        {
            if (!BehaviorCompleted)
            {
                ClearAllClaims();
                notifier.ErrandAborted(this);
            }
            ErrandBehaviorTreeRoot = null;
        }
    }
}
