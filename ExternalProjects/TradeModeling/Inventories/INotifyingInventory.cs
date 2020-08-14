using System;

namespace TradeModeling.Inventories
{
    public interface INotifyingInventory<T>
    {
        event EventHandler<ResourceChanged<T>> resourceAmountChanged;
        event EventHandler<ResourceChanged<T>> resourceCapacityChanges;

        /// <summary>
        /// Send out an event on all resources with the current values and capacities; if applicable
        /// </summary>
        void NotifyAll();
    }
}
