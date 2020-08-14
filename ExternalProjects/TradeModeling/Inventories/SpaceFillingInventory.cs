using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Economics;

namespace TradeModeling.Inventories
{
    /// <summary>
    /// An inventory with infinite capacity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SpaceFillingInventory<T> : BasicInventory<T>, ISpaceFillingInventory<T>
    {
        protected ISet<T> spaceFillingItems;
        protected ISet<T> validItems;
        public Action<float> OnCapacityChanged { private get; set; }

        public SpaceFillingInventory(
            IDictionary<T, float> initialItems,
            ICollection<T> spaceFillingItems,
            int capacity,
            ICollection<T> validItems = null) : base(initialItems)
        {
            _inventoryCapacity = capacity;
            this.spaceFillingItems = new HashSet<T>(spaceFillingItems);
            if(validItems != null)
            {
                this.validItems = new HashSet<T>(validItems);
                if (inventory.Select(x => x.Key).Except(this.validItems).Any())
                {
                    throw new ArgumentException("initial inventory cannot contain any items not in the valid item list");
                }
            }
        }

        protected SpaceFillingInventory(SpaceFillingInventory<T> other) : base(other)
        {
            _inventoryCapacity = other._inventoryCapacity;
            spaceFillingItems = new HashSet<T>(other.spaceFillingItems);
        }

        public IEnumerable<T> SpaceFillingItems => spaceFillingItems;

        private int _inventoryCapacity;
        public int inventoryCapacity
        {
            get => GetInventoryCapacity();
            set => SetInventoryCapacity(value);
        }
        public int GetInventoryCapacity()
        {
            return _inventoryCapacity;
        }

        protected virtual int SetInventoryCapacity(int newCapacity)
        {
            _inventoryCapacity = newCapacity;
            OnCapacityChanged?.Invoke(newCapacity);
            return _inventoryCapacity;
        }

        public float totalFullSpace => spaceFillingItems.Select(x => Get(x)).Sum();

        public float remainingCapacity => inventoryCapacity - totalFullSpace;

        public float getFullRatio()
        {
            return totalFullSpace / inventoryCapacity;
        }

        public override ActionOption<float> SetAmount(T type, float amount)
        {
            if (validItems != null && !validItems.Contains(type))
            {
                amount = 0;
            }else if (spaceFillingItems.Contains(type))
            {
                amount = Math.Min(amount, Get(type) + remainingCapacity);
            }
            return base.SetAmount(type, amount);
        }

        public override bool CanFitMoreOf(T resource)
        {
            if (validItems != null && !validItems.Contains(resource))
            {
                return false;
            }
            if (!spaceFillingItems.Contains(resource))
            {
                return base.CanFitMoreOf(resource);
            }
            return remainingCapacity > 0;
        }
        public override IInventory<T> CloneSimulated()
        {
            return new SpaceFillingInventory<T>(this);
        }
    }
}