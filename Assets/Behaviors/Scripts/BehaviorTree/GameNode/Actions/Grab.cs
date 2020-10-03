using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
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

        private GenericSelector<IInventory<Resource>> inventoryToGatherInto;


        private Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            GenericSelector<IInventory<Resource>> inventoryToGatherInto) : base(gameObject)
        {
            this.inventoryToGatherInto = inventoryToGatherInto;
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }

        public Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            GenericSelector<IInventory<Resource>> inventoryToGatherInto,
            string resourceTypeInBlackboard = null
            ) : this(gameObject, targetObjectInBlackboard, inventoryToGatherInto)
        {
            resourceFromBlackboard = true;
            this.resourceTypeInBlackboard = resourceTypeInBlackboard;
        }
        public Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            GenericSelector<IInventory<Resource>> inventoryToGatherInto,
            Resource resourceType
            ) : this(gameObject, targetObjectInBlackboard, inventoryToGatherInto)
        {
            resourceFromBlackboard = false;
            resourceToGrab = resourceType;
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
