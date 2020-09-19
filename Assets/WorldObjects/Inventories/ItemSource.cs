using Assets.Scripts.ObjectVariables;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    public class ItemSource : MonoBehaviour, IItemSource
    {
        public InventoryReference inventoryToProvideFrom;
        public ItemSourceType SourceType;

        public ItemSourceType ItemSourceType
        {
            get => SourceType;
        }

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

        public void GatherInto(IInventory<Resource> inventoryToGatherInto, Resource resourceType, float amount = -1)
        {
            var myInventory = inventoryToProvideFrom.CurrentValue;
            if(amount == -1)
            {
                amount = myInventory.Get(resourceType);
            }
            var transfer = myInventory.TransferResourceInto(resourceType, inventoryToGatherInto, amount);
            transfer.Execute();
        }
        public void GatherInto(IInventory<Resource> inventoryToGatherInto)
        {
            var myInventory = inventoryToProvideFrom.CurrentValue;
            myInventory.DrainAllInto(inventoryToGatherInto, myInventory.GetAllResourceTypes().ToArray());
        }

        public void GatherInto(IInventory<Resource> inventoryToGatherInto, Resource? resourceType = null, float amount = -1)
        {
            if(!resourceType.HasValue)
            {
                this.GatherInto(inventoryToGatherInto);
            }else
            {
                this.GatherInto(inventoryToGatherInto, resourceType.Value, amount);
            }
        }
    }
}
