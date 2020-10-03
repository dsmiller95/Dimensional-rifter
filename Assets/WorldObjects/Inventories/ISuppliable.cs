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
        bool CanRecieveSupply();

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
        bool IsResourceSupplyable(Resource resource);

        /// <summary>
        /// Supply into the inventory, from <paramref name="inventoryToTakeFrom"/>
        /// </summary>
        /// <param name="inventoryToTakeFrom">inventory to supply from</param>
        /// <param name="resourceType">the resource to transfer</param>
        /// <returns>true if a transfer was made, false otherwise</returns>
        bool SupplyFrom(InventoryHoldingController inventoryToTakeFrom, Resource resourceType);

        bool SupplyAllFrom(InventoryHoldingController inventoryToTakeFrom);
    }
}
