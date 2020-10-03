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
        private string resourceTypeInBlackboard;
        private string targetObjectInBlackboard;

        private Gib(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string resourceTypeInBlackboard = null
            ) : base(gameObject)
        {
            this.resourceTypeInBlackboard = resourceTypeInBlackboard;
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }
        public static BehaviorNode GibWithAnimation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string resourceTypeInBlackboard = null)
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
                    resourceTypeInBlackboard)
            );
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var suppliables = targetObject?.GetComponents<ISuppliable>();
                if (suppliables == null || suppliables.Length <= 0) return NodeStatus.FAILURE;

                if (resourceTypeInBlackboard != null &&
                    blackboard.TryGetValueOfType(resourceTypeInBlackboard, out Resource resourceType))
                {
                    foreach (var suppliable in suppliables)
                    {
                        if (suppliable.SupplyFrom(componentValue, resourceType))
                        {
                            return NodeStatus.SUCCESS;
                        };
                    }
                }
                else
                {
                    foreach (var suppliable in suppliables)
                    {
                        if (suppliable.SupplyAllFrom(componentValue))
                        {
                            return NodeStatus.SUCCESS;
                        };
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
