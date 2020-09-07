using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs
{
    /// <summary>
    /// Give items to a target object. if resource type pointer is provided only gives items of
    ///     that type. otherwise tried to give everything.
    /// </summary>
    public class Gib : ComponentMemberLeaf<TileMapNavigationMember>
    {
        private string resourceTypeInBlackboard;
        private string targetObjectInBlackboard;

        private GenericSelector<IInventory<Resource>> inventoryToGiveFrom;


        public Gib(
            GameObject gameObject,
            string targetObjectInBlackboard,
            GenericSelector<IInventory<Resource>> inventoryToGiveFrom,
            string resourceTypeInBlackboard = null
            ) : base(gameObject)
        {
            this.inventoryToGiveFrom = inventoryToGiveFrom;
            this.resourceTypeInBlackboard = resourceTypeInBlackboard;
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }

        public override NodeStatus Evaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var suppliables = targetObject?.GetComponents<Suppliable>();
                if (suppliables == null || suppliables.Length <= 0) return NodeStatus.FAILURE;

                var myVariableState = componentValue.GetComponent<VariableInstantiator>();
                var myInventory = inventoryToGiveFrom.GetCurrentValue(myVariableState);

                if (resourceTypeInBlackboard != null &&
                    blackboard.TryGetValueOfType(resourceTypeInBlackboard, out Resource resourceType))
                {
                    foreach (var suppliable in suppliables)
                    {
                        if (suppliable.SupplyInto(myInventory, resourceType))
                        {
                            return NodeStatus.SUCCESS;
                        };
                    }
                }
                else
                {
                    foreach (var suppliable in suppliables)
                    {
                        if (suppliable.SupplyInto(myInventory))
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
