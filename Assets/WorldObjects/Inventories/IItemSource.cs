using Assets.WorldObjects.Members.Hungry.HeldItems;
using System.Collections.Generic;
using TradeModeling.Inventories;

namespace Assets.WorldObjects.Inventories
{
    public interface IItemSource
    {
        ItemSourceType ItemSourceType { get; }
        IEnumerable<Resource> AvailableTypes();

        bool HasResource(Resource resource);

        void GatherAllInto(InventoryHoldingController inventoryToGatherInto);
        void GatherInto(InventoryHoldingController inventoryToGatherInto, Resource resourceType, float amount = -1);
    }
}
