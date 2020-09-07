using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers
{
    public class FindSuppliableTarget : FindTargetPath
    {
        private SuppliableType supplyTargetType;
        private GenericSelector<IInventory<Resource>> selfInventoryToUse;

        private VariableInstantiator targetVariables;

        public FindSuppliableTarget(
            GameObject gameObject,
            string pathTargetProperty,
            SuppliableType supplyTargetType,
            GenericSelector<IInventory<Resource>> selfInventoryToUse) : base(gameObject, pathTargetProperty)
        {
            this.supplyTargetType = supplyTargetType;
            this.selfInventoryToUse = selfInventoryToUse;

            targetVariables = gameObject.GetComponent<VariableInstantiator>();
        }

        protected override NavigationPath? TryGetPath(Blackboard blackboard)
        {
            var myInventory = selfInventoryToUse.GetCurrentValue(targetVariables);
            var myItems = myInventory.GetResourcesWithAny();
            var possiblePath = componentValue
                .GetClosestOfTypeWithPath(
                    member => SupplyDeliveryFilterByResourceAvailable(member, myItems)
                );
            return possiblePath;
        }

        private bool SupplyDeliveryFilterByResourceAvailable(
            TileMapMember member,
            ISet<Resource> availableResources
            )
        {
            var supplyables = member.GetComponents<Suppliable>();
            return supplyables.Any(supplyable =>
            {
                if (!SupplyDeliveryFilter(supplyable))
                {
                    return false;
                }
                return supplyable.ValidSupplyTypes().Overlaps(availableResources);
            });
        }
        private bool SupplyDeliveryFilter(Suppliable supplyable)
        {
            return supplyable?.SupplyType == supplyTargetType && supplyable.CanRecieveSupply();
        }
    }
}
