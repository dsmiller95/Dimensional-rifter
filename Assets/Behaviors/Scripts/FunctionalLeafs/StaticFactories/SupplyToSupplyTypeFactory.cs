using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.StaticFactories
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
