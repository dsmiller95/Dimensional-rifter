using Assets.Scripts.ResourceManagement;
using Assets.UI.ItemTransferAnimations;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Grab items from a target object. if resource type pointer is provided only gathers items of
    ///     that type. otherwise tried to gather everything.
    /// </summary>
    public class Grab : ComponentMemberLeaf<InventoryHoldingController>
    {
        private string targetObjectInBlackboard;

        private bool getClaimFromBlackboard;
        private string grabClaimInBlackboard;
        private ResourceAllocation grabClaim;

        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard) : base(gameObject)
        {
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }
        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string grabClaimInBlackboard
            ) : this(gameObject, targetObjectInBlackboard)
        {
            getClaimFromBlackboard = true;
            this.grabClaimInBlackboard = grabClaimInBlackboard;
        }
        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            ResourceAllocation grabClaim) : this(gameObject, targetObjectInBlackboard)
        {
            getClaimFromBlackboard = false;
            this.grabClaim = grabClaim;
        }

        public static BehaviorNode GrabWithAnimation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string grabClaimInBlackboard)
        {
            var grabAction = new Grab(gameObject,
                targetObjectInBlackboard,
                grabClaimInBlackboard);
            return WrapWithAnimation(
                grabAction,
                targetObjectInBlackboard,
                gameObject
                );
        }

        public static BehaviorNode GrabWithBakedAllocation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            ResourceAllocation resourceAllocation)
        {
            var grabAction = new Grab(gameObject,
                targetObjectInBlackboard,
                resourceAllocation);
            return WrapWithAnimation(
                grabAction,
                targetObjectInBlackboard,
                gameObject
                );
        }

        public static BehaviorNode WrapWithAnimation(
            BehaviorNode grabNode,
            string targetObjectBlackboard,
            GameObject target)
        {
            return new Sequence(
                new LabmdaLeaf(blackboard =>
                {
                    if (blackboard.TryGetValueOfType(targetObjectBlackboard, out GameObject obj))
                    {
                        ItemTransferParticleProvider.ShowItemTransferAnimation(obj, target);
                    }
                    return NodeStatus.SUCCESS;
                }),
                new Wait(ItemTransferParticleProvider.Instance.ItemTransferAnimationTime),
                grabNode
            );
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (!blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                return NodeStatus.FAILURE;
            }
            var supplier = targetObject?.GetComponent<IItemSource>();
            if (supplier == null) return NodeStatus.FAILURE;

            var claim = grabClaim;
            if (getClaimFromBlackboard)
            {
                if (!blackboard.TryGetValueOfType(grabClaimInBlackboard, out claim))
                {
                    return NodeStatus.FAILURE;
                }
            }
            supplier.GatherInto(componentValue, claim);

            return NodeStatus.SUCCESS;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
