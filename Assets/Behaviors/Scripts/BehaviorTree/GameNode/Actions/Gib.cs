using Assets.Scripts.ResourceManagement;
using Assets.UI.ItemTransferAnimations;
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
        private string targetObjectInBlackboard;

        private bool getClaimFromBlackboard;
        private string gibClaimInBlackboard;
        private ResourceAllocation gibClaim;

        private Gib(
            GameObject gameObject,
            string targetObjectInBlackboard) : base(gameObject)
        {
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }
        private Gib(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string gibClaimInBlackboard
            ) : this(gameObject, targetObjectInBlackboard)
        {
            getClaimFromBlackboard = true;
            this.gibClaimInBlackboard = gibClaimInBlackboard;
        }
        private Gib(
            GameObject gameObject,
            string targetObjectInBlackboard,
            ResourceAllocation gibClaim
            ) : this(gameObject, targetObjectInBlackboard)
        {
            getClaimFromBlackboard = false;
            this.gibClaim = gibClaim;
        }
        public static BehaviorNode GibWithAnimation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string gibClaimInBlackboard)
        {
            var gib = new Gib(
                gameObject,
                targetObjectInBlackboard,
                gibClaimInBlackboard);
            return WrapWithAnimation(gib,
                targetObjectInBlackboard,
                gameObject);
        }
        public static BehaviorNode GibWithBakedAllocation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            ResourceAllocation resourceAllocation)
        {
            var gib = new Gib(
                gameObject,
                targetObjectInBlackboard,
                resourceAllocation);
            return WrapWithAnimation(gib,
                targetObjectInBlackboard,
                gameObject);
        }

        private static BehaviorNode WrapWithAnimation(
            BehaviorNode gibNode,
            string targetObjectBlackboard,
            GameObject target
            )
        {
            return new Sequence(
                new LabmdaLeaf(blackboard =>
                {
                    if (blackboard.TryGetValueOfType(targetObjectBlackboard, out GameObject obj))
                    {
                        ItemTransferParticleProvider.ShowItemTransferAnimation(target, obj);
                    }
                    return NodeStatus.SUCCESS;
                }),
                new Wait(ItemTransferParticleProvider.Instance.ItemTransferAnimationTime),
                gibNode
            );
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var suppliables = targetObject?.GetComponents<ISuppliable>();
                if (suppliables == null || suppliables.Length <= 0) return NodeStatus.FAILURE;

                var claim = gibClaim;
                if (getClaimFromBlackboard)
                {
                    if (!blackboard.TryGetValueOfType(gibClaimInBlackboard, out claim))
                    {
                        return NodeStatus.FAILURE;
                    }
                }
                if (claim == null)
                {
                    return NodeStatus.FAILURE;
                }
                foreach (var suppliable in suppliables)
                {
                    if (!suppliable.IsClaimValidForThisSuppliable(claim))
                    {
                        continue;
                    }
                    if (suppliable.SupplyFrom(componentValue, claim))
                    {
                        return NodeStatus.SUCCESS;
                    }
                }
                claim.Release();
            }
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
