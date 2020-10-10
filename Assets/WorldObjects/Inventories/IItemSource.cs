using Assets.Scripts.ResourceManagement;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using System.Collections.Generic;
using static Assets.Scripts.ResourceManagement.LimitedResourcePool;

namespace Assets.WorldObjects.Inventories
{
    public interface IItemSource
    {
        ItemSourceType ItemSourceType { get; }
        IEnumerable<Resource> ClaimableTypes();

        bool HasClaimableResource(Resource resource);
        ResourceAllocation ClaimSubtractionFromSource(Resource resourceType, float amount = -1);

        void GatherInto(
            InventoryHoldingController inventoryToGatherInto,
            Resource resourceType, // TODO: shouldn't need this anymore. the resource type is stored in the allocation
            ResourceAllocation amount);
    }
}
