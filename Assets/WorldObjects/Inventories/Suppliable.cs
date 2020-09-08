using Assets.Scripts.Core;
using Assets.Scripts.ObjectVariables;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    public class Suppliable : MonoBehaviour
    {
        public BooleanReference IsSupplyable;
        public InventoryReference inventoryToSupplyInto;
        public SuppliableType SupplyType;

        public BooleanReference SupplyFull;


        public bool CanRecieveSupply()
        {
            return IsSupplyable.CurrentValue && !SupplyFull.CurrentValue;
        }

        public ISet<Resource> ValidSupplyTypes()
        {
            var inv = inventoryToSupplyInto.CurrentValue;
            return inv.GetResourcesWithSpace();
        }

        /// <summary>
        /// If this suppliable can be supplied more of a given resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public bool IsResourceSupplyable(Resource resource)
        {
            var inv = inventoryToSupplyInto.CurrentValue;
            return inv.CanFitMoreOf(resource);
        }

        public bool SupplyInto(IInventory<Resource> inventoryToTakeFrom, Resource? resourceType = null)
        {
            if (!CanRecieveSupply())
            {
                return false;
            }

            var myInventory = inventoryToSupplyInto.CurrentValue;
            var anyTransferred = false;
            if (resourceType.HasValue)
            {
                anyTransferred = inventoryToTakeFrom
                    .DrainAllInto(myInventory, resourceType.Value)
                    .Any(pair => pair.Value > 1e-5);
            }
            else
            {
                anyTransferred = inventoryToTakeFrom
                    .DrainAllInto(myInventory, myInventory.GetAllResourceTypes().ToArray())
                    .Any(pair => pair.Value > 1e-5);
            }

            if (myInventory is ISpaceFillingInventoryAccess<Resource> spaceFillingInv)
            {
                if (spaceFillingInv.remainingCapacity <= 1e-5)
                {
                    SupplyFull.SetValue(true);
                }
            }

            return anyTransferred;
        }
    }
}
