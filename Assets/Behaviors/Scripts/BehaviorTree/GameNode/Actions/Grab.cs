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

        private bool resourceFromBlackboard;
        private string resourceTypeInBlackboard;
        private Resource resourceToGrab;

        private string targetObjectInBlackboard;


        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard) : base(gameObject)
        {
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }

        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string resourceTypeInBlackboard = null
            ) : this(gameObject, targetObjectInBlackboard)
        {
            resourceFromBlackboard = true;
            this.resourceTypeInBlackboard = resourceTypeInBlackboard;
        }
        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            Resource resourceType
            ) : this(gameObject, targetObjectInBlackboard)
        {
            resourceFromBlackboard = false;
            resourceToGrab = resourceType;
        }
        public static BehaviorNode GrabWithAnimation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            string resourceTypeInBlackboard = null)
        {
            return WrapWithAnimation(
                new Grab(
                    gameObject,
                    targetObjectInBlackboard,
                    resourceTypeInBlackboard),
                targetObjectInBlackboard,
                gameObject
                );
        }

        public static BehaviorNode GrabWithAnimation(
            GameObject gameObject,
            string targetObjectInBlackboard,
            Resource resourceType)
        {
            return WrapWithAnimation(
                new Grab(
                    gameObject,
                    targetObjectInBlackboard,
                    resourceType),
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

                var resource = GetResource(blackboard);
                if (resource.HasValue)
                {
                    supplier.GatherInto(inventoryHolder, resource.Value);
                }
                else
                {
                    supplier.GatherAllInto(inventoryHolder);
                }

                return NodeStatus.SUCCESS;
            }
            return NodeStatus.FAILURE;
        }

        private Resource? GetResource(Blackboard blackboard)
        {
            if (resourceFromBlackboard &&
                resourceTypeInBlackboard != null &&
                blackboard.TryGetValueOfType(resourceTypeInBlackboard, out Resource resourceType))
            {
                return resourceType;
            }
            if (!resourceFromBlackboard)
            {
                return resourceToGrab;
            }
            return null;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
