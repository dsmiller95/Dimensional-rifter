﻿using System.Collections.Generic;
using TradeModeling.Inventories;

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

        bool SupplyInto(IInventory<Resource> inventoryToTakeFrom, Resource? resourceType = null);
    }
}