using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.ResourceManagement;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    public class StoreErrand : IErrand
    {
        private StoreErrandType errandType;
        public ErrandType ErrandType => errandType;
        private BehaviorNode ErrandBehaviorTreeRoot;

        private IItemSource itemSource;
        private ISuppliable supplyTarget;

        private Resource resourceToTransfer;
        private float amountToTransfer;


        public GameObject storingWorker;

        private IErrandCompletionReciever<StoreErrand> notifier;
        private bool BehaviorCompleted = false;

        private ResourceAllocation grabAllocation;
        private ResourceAllocation gibAllocation;

        public StoreErrand(
            StoreErrandType errandType,
            IItemSource itemSource,
            ISuppliable supplyTarget,
            Resource resourceToTransfer,
            float amountToTransfer,
            GameObject actor,
            IErrandCompletionReciever<StoreErrand> notifier)
        {
            this.errandType = errandType;
            this.itemSource = itemSource;
            this.supplyTarget = supplyTarget;
            this.resourceToTransfer = resourceToTransfer;
            this.amountToTransfer = amountToTransfer;

            storingWorker = actor;

            this.notifier = notifier;

            SetupBehavior();
        }

        private void SetupBehavior()
        {
            grabAllocation = itemSource.ClaimSubtractionFromSource(resourceToTransfer, amountToTransfer);
            gibAllocation = supplyTarget.ClaimAdditionToSuppliable(resourceToTransfer, amountToTransfer);
            var actualTransferAmount = Mathf.Min(grabAllocation.Amount, gibAllocation.Amount);
            //TODO: ensure that the allocation is no bigger than it has to be, instead of adjusting the real amount
            //  based on the min
            ErrandBehaviorTreeRoot =
            new Sequence(
                new FindPathToBakedTarget(
                    storingWorker,
                    (itemSource as Component).gameObject,
                    "Path",
                    true),
                new NavigateToTarget(
                    storingWorker,
                    "Path",
                    "target"),
                Grab.GrabWithBakedAllocation(
                    storingWorker,
                    "target",
                    grabAllocation),
                new FindPathToBakedTarget(
                    storingWorker,
                    (supplyTarget as Component).gameObject,
                    "Path",
                    true),
                new NavigateToTarget(
                    storingWorker,
                    "Path",
                    "target"),
                Gib.GibWithBakedAllocation(
                    storingWorker,
                    "target",
                    gibAllocation),
                new LabmdaLeaf(blackboard =>
                {
                    BehaviorCompleted = true;
                    notifier.ErrandCompleted(this);
                    return NodeStatus.SUCCESS;
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
            grabAllocation.Release();
            gibAllocation.Release();
            grabAllocation = gibAllocation = null;
            //TODO: drop everything if interrupted?
        }
    }
}
