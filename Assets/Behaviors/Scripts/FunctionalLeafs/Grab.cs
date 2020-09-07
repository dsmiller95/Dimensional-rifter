using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Food;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs
{
    /// <summary>
    /// Grab items from a target object. if resource type pointer is provided only gathers items of
    ///     that type. otherwise tried to gather everything.
    /// </summary>
    public class Grab : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private string resourceTypeInBlackboard;
        private string targetObjectInBlackboard;

        private GenericSelector<IInventory<Resource>> inventoryToGatherInto;


        public Grab(
            GameObject gameObject,
            string targetObjectInBlackboard,
            GenericSelector<IInventory<Resource>> inventoryToGatherInto,
            string resourceTypeInBlackboard = null
            ) : base(gameObject)
        {
            this.inventoryToGatherInto = inventoryToGatherInto;
            this.resourceTypeInBlackboard = resourceTypeInBlackboard;
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var supplier = targetObject?.GetComponent<ItemSource>();
                if (supplier == null) return NodeStatus.FAILURE;

                var myVariableState = componentValue.GetComponent<VariableInstantiator>();
                var myInventory = inventoryToGatherInto.GetCurrentValue(myVariableState);
                if (resourceTypeInBlackboard != null &&
                    blackboard.TryGetValueOfType(resourceTypeInBlackboard, out Resource resourceType))
                {
                    supplier.GatherInto(myInventory, resourceType);
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

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
