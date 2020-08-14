using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Economics;

namespace TradeModeling.Inventories
{
    public enum CompositeDistributionMode
    {
        EVEN,
        EVEN_SNAP_TO_INT,
        IN_ORDER
    }

    /// <summary>
    /// An inventory which only stores items in a list of other inventory sources
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompositeInventory<T> : ISpaceFillingInventoryAccess<T>
    {
        private IList<IInventory<T>> inventorySources;

        public Action<T, float> OnResourceChanged { private get; set; }


        private ISet<T> AllResourceTypes;
        private CompositeDistributionMode distributionMode;

        public CompositeInventory(IList<IInventory<T>> composingInventories, CompositeDistributionMode mode)
        {
            inventorySources = composingInventories;
            distributionMode = mode;
            AllResourceTypes = new HashSet<T>();
            foreach (var inv in inventorySources)
            {
                foreach (var type in inv.GetAllResourceTypes())
                {
                    AllResourceTypes.Add(type);
                }
            }
        }

        public Dictionary<T, float> GetCurrentResourceAmounts()
        {
            var result = new Dictionary<T, float>();
            foreach (var inv in inventorySources)
            {
                foreach (var amount in inv.GetCurrentResourceAmounts())
                {
                    if (!result.ContainsKey(amount.Key))
                    {
                        result[amount.Key] = 0f;
                    }
                    result[amount.Key] += amount.Value;
                }
            }
            return result;
        }

        public float Get(T type)
        {
            return inventorySources.Sum(inv => inv.Get(type));
        }

        public IEnumerable<T> GetAllResourceTypes()
        {
            return AllResourceTypes;
        }

        public virtual ActionOption<float> SetAmount(T type, float amount)
        {
            var inOrderOptions = SetInventoryValueInOrder(type, amount).ToList();
            var totalSpace = inOrderOptions.Sum(x => x.info);

            // cannot have a negative amount
            var nonNegativeAmount = Math.Max(amount, 0);
            var newAmount = Math.Min(nonNegativeAmount, totalSpace);
            return new ActionOption<float>(newAmount, () =>
            {
                switch (distributionMode)
                {
                    case CompositeDistributionMode.EVEN:
                        SetInventoryValueEvenlyDistributed(type, newAmount, false);
                        break;

                    case CompositeDistributionMode.EVEN_SNAP_TO_INT:
                        SetInventoryValueEvenlyDistributed(type, newAmount, true);
                        break;
                    case CompositeDistributionMode.IN_ORDER:
                        foreach (var option in inOrderOptions)
                        {
                            option.Execute();
                        }
                        break;
                }
                OnResourceChanged?.Invoke(type, newAmount);
            });
        }

        private IEnumerable<ActionOption<float>> SetInventoryValueInOrder(T type, float newValue)
        {
            foreach (var inventory in inventorySources)
            {
                var option = inventory.SetAmount(type, newValue);
                yield return option;
                newValue -= option.info;
                if (newValue <= 1e-5)
                {
                    yield break;
                }
            }
        }

        private void SetInventoryValueEvenlyDistributed(T type, float newValue, bool stickInts)
        {
            var inventorySourcesToFill = inventorySources;
            foreach (var inventory in inventorySourcesToFill)
            {
                inventory.SetAmount(type, 0f).Execute();
            }

            var totalItemsToAllocate = newValue;
            inventorySourcesToFill = inventorySourcesToFill.Where(x => x.CanFitMoreOf(type)).ToList();

            while (totalItemsToAllocate > 0 && inventorySourcesToFill.Count > 0)
            {
                var averageAmountToAdd = totalItemsToAllocate / inventorySourcesToFill.Count;
                if (stickInts && (averageAmountToAdd = (float)Math.Floor(averageAmountToAdd)) < 1)
                {
                    SetInventoryValueBySteps(type, totalItemsToAllocate, inventorySourcesToFill, 1);
                    return;
                }
                foreach (var inventory in inventorySourcesToFill)
                {
                    var addOption = inventory.Add(type, averageAmountToAdd);
                    totalItemsToAllocate -= addOption.info;
                    addOption.Execute();
                }
                inventorySourcesToFill = inventorySourcesToFill.Where(x => x.CanFitMoreOf(type)).ToList();
            }
        }

        private void SetInventoryValueBySteps(T type, float totalItemsToAllocate, IList<IInventory<T>> inventorySourcesToFill, float step)
        {
            inventorySourcesToFill = inventorySourcesToFill.Where(x => x.CanFitMoreOf(type)).ToList();

            while (totalItemsToAllocate > 0 && inventorySourcesToFill.Count > 0)
            {
                foreach (var inventory in inventorySourcesToFill)
                {
                    var amountToAdd = Math.Min(step, totalItemsToAllocate);
                    var addOption = inventory.Add(type, amountToAdd);
                    totalItemsToAllocate -= addOption.info;
                    addOption.Execute();
                    if (totalItemsToAllocate <= 1e-5)
                    {
                        return;
                    }
                }
                inventorySourcesToFill = inventorySourcesToFill.Where(x => x.CanFitMoreOf(type)).ToList();
            }
        }
        public virtual bool CanFitMoreOf(T resource)
        {
            return inventorySources.Any(x => x.CanFitMoreOf(resource));
        }
        public virtual IInventory<T> CloneSimulated()
        {
            throw new NotImplementedException();
            //TODO: hmmmm
            //return new BasicInventorySource<T>(this);
        }

        public string ToString(Func<T, string> serializer)
        {
            return MyUtilities.SerializeDictionary(GetCurrentResourceAmounts(), serializer, num => num.ToString());
        }

        private bool HasInfiniteCapacity()
        {
            return inventorySources.Any(x => !(x is ISpaceFillingInventoryAccess<T>));
        }

        public float totalFullSpace
        {
            get
            {
                if (HasInfiniteCapacity())
                {
                    return 0;
                }
                return inventorySources.OfType<ISpaceFillingInventoryAccess<T>>().Sum(x => x.totalFullSpace);
            }
        }

        public float remainingCapacity
        {
            get
            {
                if (HasInfiniteCapacity())
                {
                    return int.MaxValue;
                }
                return inventorySources.OfType<ISpaceFillingInventoryAccess<T>>().Sum(x => x.remainingCapacity);
            }
        }

        public int GetInventoryCapacity()
        {
            if (HasInfiniteCapacity())
            {
                return int.MaxValue;
            }

            return inventorySources.OfType<ISpaceFillingInventoryAccess<T>>().Sum(x => x.GetInventoryCapacity());
        }

        public float getFullRatio()
        {
            return totalFullSpace / GetInventoryCapacity();
        }
    }
}