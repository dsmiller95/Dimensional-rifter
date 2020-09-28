using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Give items to a target object. if resource type pointer is provided only gives items of
    ///     that type. otherwise tried to give everything.
    /// </summary>
    public class Gib : ComponentMemberLeaf<VariableInstantiator>
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

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var suppliables = targetObject?.GetComponents<ISuppliable>();
                if (suppliables == null || suppliables.Length <= 0) return NodeStatus.FAILURE;

                var myInventory = inventoryToGiveFrom.GetCurrentValue(componentValue);

                if (resourceTypeInBlackboard != null &&
                    blackboard.TryGetValueOfType(resourceTypeInBlackboard, out Resource resourceType))
                {
                    foreach (var suppliable in suppliables)
                    {
                        if (suppliable.SupplyFrom(myInventory, resourceType))
                        {
                            return NodeStatus.SUCCESS;
                        };
                    }
                }
                else
                {
                    foreach (var suppliable in suppliables)
                    {
                        if (suppliable.SupplyFrom(myInventory))
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
