using Assets.Scripts.Core;
using Assets.Scripts.ObjectVariables;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    public class Supplyable : MonoBehaviour
    {
        public BooleanReference IsSupplyable;
        public InventoryReference inventoryToSupplyInto;
        public SupplyType SupplyType;

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

        public bool IsResourceSupplyable(Resource resource)
        {
            var inv = inventoryToSupplyInto.CurrentValue;
            return inv.CanFitMoreOf(resource);
        }

        public void SupplyInto(IInventory<Resource> inventoryToTakeFrom)
        {
            if (!CanRecieveSupply())
            {
                return;
            }

            var myInventory = inventoryToSupplyInto.CurrentValue;
            inventoryToTakeFrom.DrainAllInto(myInventory, myInventory.GetAllResourceTypes().ToArray());
            if (myInventory is ISpaceFillingInventoryAccess<Resource> spaceFillingInv)
            {
                if(spaceFillingInv.remainingCapacity <= 1e-5)
                {
                    SupplyFull.SetValue(true);
                }
            }
        }
    }
}
