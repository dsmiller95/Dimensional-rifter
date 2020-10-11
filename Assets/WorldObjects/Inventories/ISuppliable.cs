using Assets.Scripts.ResourceManagement;
using Assets.WorldObjects.Members.Hungry.HeldItems;
using System.Collections.Generic;

namespace Assets.WorldObjects.Inventories
{
    /// <summary>
    /// represents something that can be supplied into
    /// </summary>
    public interface ISuppliable
    {
        SuppliableType SuppliableClassification { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>If this suppliable can recieve any items</returns>
        bool CanClaimSpaceForAny();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>What items this suppliable can currently accept</returns>
        ISet<Resource> ValidSupplyTypes();

        /// <summary>
        /// If this suppliable can be supplied more of a given resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        bool CanClaimSpaceForMoreOf(Resource resource);
        ResourceAllocation ClaimAdditionToSuppliable(Resource resourceType, float amount);

        /// <summary>
        /// Supply into the inventory, from <paramref name="inventoryToTakeFrom"/>
        ///     Should ensure that the allocation is released regardless of the ability to transfer
        /// </summary>
        /// <param name="inventoryToTakeFrom">inventory to supply from</param>
        /// <param name="resourceType">the resource to transfer</param>
        /// <returns>true if a transfer was made, false otherwise</returns>
        bool SupplyFrom(
            InventoryHoldingController inventoryToTakeFrom,
            ResourceAllocation amount);
    }
}
