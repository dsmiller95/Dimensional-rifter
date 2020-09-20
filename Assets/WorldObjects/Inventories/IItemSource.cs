using System.Collections.Generic;
using TradeModeling.Inventories;

namespace Assets.WorldObjects.Inventories
{
    public interface IItemSource
    {
        ItemSourceType ItemSourceType { get; }
        IEnumerable<Resource> AvailableTypes();

        bool HasResource(Resource resource);

        void GatherInto(IInventory<Resource> inventoryToGatherInto, Resource? resourceType = null, float amount = -1);
    }
}
