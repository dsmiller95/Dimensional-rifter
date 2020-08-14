using System;
using System.Collections.Generic;

namespace TradeModeling.Inventories
{
    /// <summary>
    /// Represents and extension of the most basic inventory item source. This interface only exposes
    ///     methods which provide information about the current capacity, with no ability to modify the 
    ///     capacity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISpaceFillingInventoryAccess<T> : IInventory<T>
    {
        int GetInventoryCapacity();
        float totalFullSpace { get; }
        float remainingCapacity { get; }
        float getFullRatio();
    }
    /// <summary>
    /// Represents and extension of the most basic inventory item source. It can contain items
    ///    and has a set capacity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISpaceFillingInventory<T> : ISpaceFillingInventoryAccess<T>
    {
        Action<float> OnCapacityChanged { set; }
        IEnumerable<T> SpaceFillingItems { get; }
        int inventoryCapacity { get;  set; }
    }
}
