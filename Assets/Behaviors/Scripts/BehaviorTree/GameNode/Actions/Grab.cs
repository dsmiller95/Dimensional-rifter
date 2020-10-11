using Assets.UI.ItemTransferAnimations;
using Assets.WorldObjects;
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
    public class Grab : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private float grabAmount;
        private Resource resourceToGrab;

        private string targetObjectInBlackboard;


        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            Resource resource,
            float grabAmount) : base(gameObject)
        {
            this.targetObjectInBlackboard = targetObjectInBlackboard;
            this.grabAmount = grabAmount;
            this.resourceToGrab = resource;
        }

        public static BehaviorNode GrabWithAnimation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            Resource resourceType,
            float grabAmount = -1)
        {
            var grabAction = new Grab(gameObject,
                targetObjectInBlackboard,
                resourceType,
                grabAmount);
            return WrapWithAnimation(
                grabAction,
                targetObjectInBlackboard,
                gameObject
                );
        }

        private static BehaviorNode WrapWithAnimation(
            Grab grabNode,
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
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var supplier = targetObject?.GetComponent<IItemSource>();
                if (supplier == null) return NodeStatus.FAILURE;

                var inventoryHolder = componentValue.GetComponent<InventoryHoldingController>();

                var allocation = supplier.ClaimSubtractionFromSource(resourceToGrab, this.grabAmount);
                if(allocation == null)
                {
                    return NodeStatus.FAILURE;
                }
                supplier.GatherInto(inventoryHolder, allocation);

                return NodeStatus.SUCCESS;
            }
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
