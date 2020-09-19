using Assets.Scripts.ObjectVariables;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    public interface IItemSource
    {
        //public InventoryReference inventoryToProvideFrom;
        //public ItemSourceType SourceType;

        ItemSourceType ItemSourceType { get; }
        IEnumerable<Resource> AvailableTypes();

        bool HasResource(Resource resource);

        void GatherInto(IInventory<Resource> inventoryToGatherInto, Resource? resourceType = null, float amount = -1);
    }
}
