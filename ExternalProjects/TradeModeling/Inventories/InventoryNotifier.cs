using System;
using System.Collections.Generic;

namespace TradeModeling.Inventories
{
    public struct ResourceChanged<T>
    {
        public ResourceChanged(T type, float newValue)
        {
            this.type = type;
            this.newValue = newValue;
        }
        public T type;
        public float newValue;
    }
    /// <summary>
    /// An object which wraps an inventory item source and sends out notifications when it changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InventoryNotifier<T> : INotifyingInventory<T>
    {
        protected IInventory<T> backingSource;
        private float defaultCapacity;

        public event EventHandler<ResourceChanged<T>> resourceAmountChanged;
        public event EventHandler<ResourceChanged<T>> resourceCapacityChanges;

        public InventoryNotifier(IInventory<T> backing, float defaultCapacity)
        {
            backingSource = backing;
            this.defaultCapacity = defaultCapacity;

            backingSource.OnResourceChanged = (resource, amount) =>
            {
                resourceAmountChanged?.Invoke(backingSource, new ResourceChanged<T>
                {
                    newValue = amount,
                    type = resource
                });
            };

            if (backingSource is ISpaceFillingInventory<T> spaceFilling)
            {
                spaceFilling.OnCapacityChanged = (amount) =>
                {
                    NotifyAllCapacityChange();
                };
            }
        }

        private void NotifyAllCapacityChange()
        {
            var resourcesToSet = new HashSet<T>(backingSource.GetAllResourceTypes());
            if (backingSource is ISpaceFillingInventory<T> spaceFilling)
            {
                foreach (var resource in spaceFilling.SpaceFillingItems)
                {
                    resourcesToSet.Remove(resource);
                    resourceCapacityChanges?.Invoke(this, new ResourceChanged<T>(resource, spaceFilling.inventoryCapacity));
                }
            }
            foreach (var resource in resourcesToSet)
            {
                resourceCapacityChanges?.Invoke(this, new ResourceChanged<T>(resource, defaultCapacity));
            }
        }

        public void NotifyAll()
        {
            NotifyAllCapacityChange();

            foreach (var resource in backingSource.GetAllResourceTypes())
            {
                resourceAmountChanged?.Invoke(this, new ResourceChanged<T>(resource, backingSource.Get(resource)));
            }
        }
    }
}