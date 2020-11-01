using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.ResourceManagement;
using Assets.UI.Buttery_Toast;
using Assets.UI.ItemTransferAnimations;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Inventories;
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
                            // TODO: actually transfer things, and release the claim on the resource
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
                            // TODO: actually transfer things, and release the claim on the resource
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
                    Debug.Log("storage errand failed, clearing allocations");
                    //grabAllocation.Release();
                    //gibAllocation.Release();
                    return NodeStatus.FAILURE;
                })
            );
        }

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }

        public void OnReset()
        {
            if (!BehaviorCompleted)
            {
                notifier.ErrandAborted(this);
            }
            ErrandBehaviorTreeRoot = null;
            //grabAllocation = gibAllocation = null;
            //TODO: drop everything if interrupted?
        }
    }
}
