using Assets.UI.ItemTransferAnimations;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Give items to a target object. if resource type pointer is provided only gives items of
    ///     that type. otherwise tried to give everything.
    /// </summary>
    public class Gib : ComponentMemberLeaf<InventoryHoldingController>
    {
        private Resource resourceToTransfer;
        private float gibAmount;
        private string targetObjectInBlackboard;

        private Gib(
            GameObject gameObject,
            string targetObjectInBlackboard,
            Resource resourceToTransfer,
            float gibAmount
            ) : base(gameObject)
        {
            this.resourceToTransfer = resourceToTransfer;
            this.targetObjectInBlackboard = targetObjectInBlackboard;
            this.gibAmount = gibAmount;
        }
        public static BehaviorNode GibWithAnimation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            Resource resourceToTransfer,
            float gibAmount)
        {
            return new Sequence(
                new LabmdaLeaf(blackboard =>
                {
                    if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject obj))
                    {
                        ItemTransferParticleProvider.ShowItemTransferAnimation(gameObject, obj);
                    }
                    return NodeStatus.SUCCESS;
                }),
                new Wait(ItemTransferParticleProvider.Instance.ItemTransferAnimationTime),
                new Gib(
                    gameObject,
                    targetObjectInBlackboard,
                    resourceToTransfer,
                    gibAmount)
            );
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var suppliables = targetObject?.GetComponents<ISuppliable>();
                if (suppliables == null || suppliables.Length <= 0) return NodeStatus.FAILURE;

                foreach (var suppliable in suppliables)
                {
                    var allocation = suppliable.ClaimAdditionToSuppliable(resourceToTransfer, gibAmount);
                    if (allocation != null)
                    {
                        if (suppliable.SupplyFrom(componentValue, allocation))
                        {
                            return NodeStatus.SUCCESS;
                        }
                    }
                }
            }
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
