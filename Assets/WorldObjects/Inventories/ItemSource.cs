using Assets.Scripts.ObjectVariables;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    public class ItemSource : MonoBehaviour
    {
        public InventoryReference inventoryToProvideFrom;
        public ItemSourceType SourceType;

        public IEnumerable<Resource> AvailableTypes()
        {
            var inv = inventoryToProvideFrom.CurrentValue;
            return inv.GetCurrentResourceAmounts().Where(x => x.Value > 0).Select(x => x.Key);
        }

        public bool HasResource(Resource resource)
        {
            var inv = inventoryToProvideFrom.CurrentValue;
            return inv.Get(resource) > 0;
        }

        public void GatherInto(IInventory<Resource> inventoryToGatherInto, Resource resourceType)
        {
            var myInventory = inventoryToProvideFrom.CurrentValue;
            var transfer = myInventory.TransferResourceInto(resourceType, inventoryToGatherInto, myInventory.Get(resourceType));
            transfer.Execute();
        }
        public void GatherInto(IInventory<Resource> inventoryToGatherInto)
        {
            var myInventory = inventoryToProvideFrom.CurrentValue;
            myInventory.DrainAllInto(inventoryToGatherInto, myInventory.GetAllResourceTypes().ToArray());
        }
    }
}
