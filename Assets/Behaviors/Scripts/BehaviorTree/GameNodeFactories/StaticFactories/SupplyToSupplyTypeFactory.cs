using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    public static class SupplyToSupplyTypeFactory
    {
        public static Node SupplyAnyResourceFromSelfToSuppliable(
            GameObject target,
            SuppliableType supplyTargetType,
            GenericSelector<IInventory<Resource>> inventoryToSupplyFrom,
            string tempPathProp = "itemSourcePath",
            string targetReachedProp = "itemSourceObject")
        {
            return
            new Sequence(
                new FindSuppliableTarget(
                    target,
                    tempPathProp,
                    supplyTargetType,
                    inventoryToSupplyFrom),
                new NavigateToTarget(
                    target,
                    tempPathProp,
                    targetReachedProp),
                new Gib(
                    target,
                    targetReachedProp,
                    inventoryToSupplyFrom
                    )
            );
        }
    }
}
