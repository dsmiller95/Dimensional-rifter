using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Food;
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

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var supplier = targetObject?.GetComponent<ItemSource>();
                if (supplier == null) return NodeStatus.FAILURE;

                var myVariableState = componentValue.GetComponent<VariableInstantiator>();
                var myInventory = inventoryToGatherInto.GetCurrentValue(myVariableState);

                var resource = GetResource(blackboard);
                if (resource.HasValue)
                {
                    supplier.GatherInto(myInventory, resource.Value);
                }
                else
                {
                    supplier.GatherInto(myInventory);
                }

                // todo: no more gathering!
                targetObject.GetComponent<IGatherable>()?.OnGathered();

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
