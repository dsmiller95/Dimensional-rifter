using Assets.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.ResourceManagement
{
    [Serializable]
    public class LimitedMultiResourcePoolSaveObject
    {
        public float maxCapacity;
        public List<ResourceAmount> currentAmounts;

        [Serializable]
        public struct ResourceAmount
        {
            public Resource type;
            public float amount;
        }
    }

    public class LimitedMultiResourcePool
    {
        private float maxCapacity;
        private IDictionary<Resource, float> itemAmounts;
        private IDictionary<Resource, float> totalAllocatedSubtractions;

        private float totalAllocatedAdditions;

        public LimitedMultiResourcePool(
            int capacity,
            IDictionary<Resource, float> initialItems)
        {
            totalAllocatedSubtractions = new Dictionary<Resource, float>();
            itemAmounts = new Dictionary<Resource, float>(initialItems);
            maxCapacity = capacity;
        }

        public LimitedMultiResourcePool(int capacity) : this(capacity, new Dictionary<Resource, float>()) { }

        public LimitedMultiResourcePool(LimitedMultiResourcePoolSaveObject saveObject)
        {
            totalAllocatedSubtractions = new Dictionary<Resource, float>();
            itemAmounts = saveObject.currentAmounts.ToDictionary(x => x.type, x => x.amount);
            maxCapacity = saveObject.maxCapacity;
        }

        public LimitedMultiResourcePoolSaveObject GetSaveObject()
        {
            return new LimitedMultiResourcePoolSaveObject
            {
                maxCapacity = maxCapacity,
                currentAmounts = itemAmounts.Select(x => new LimitedMultiResourcePoolSaveObject.ResourceAmount
                {
                    amount = x.Value,
                    type = x.Key
                }).ToList()
            };
        }

        public float totalFullSpace => itemAmounts.Sum(x => x.Value);

        public float remainingCapacity => maxCapacity - totalFullSpace;

        public float getFullRatio()
        {
            return totalFullSpace / maxCapacity;
        }

        #region Addition Allocation
        public bool CanAllocateAddition()
        {
            return (totalFullSpace + totalAllocatedAdditions) < maxCapacity;
        }
        public AdditionAllocation TryAllocateAddition(Resource type, float amount)
        {
            var currentFullSpace = totalFullSpace;
            if (amount + totalAllocatedAdditions + currentFullSpace > maxCapacity)
            {
                amount = maxCapacity - (totalAllocatedAdditions + currentFullSpace);
            }
            if (amount < 0)
            {
                return null;
            }
            return new AdditionAllocation(amount, type, this);
        }
        public class AdditionAllocation : ResourceAllocation
        {
            private LimitedMultiResourcePool target;
            public Resource type { get; private set; }
            public AdditionAllocation(
                float amount,
                Resource type,
                LimitedMultiResourcePool target) : base(amount, target)
            {
                this.target = target;
                this.type = type;
                target.totalAllocatedAdditions += amount;
            }

            protected override bool DoExecute(float actualAmount)
            {
                var amountToAdd = Math.Min(Amount, actualAmount);
                if (amountToAdd < 0 ||
                    amountToAdd > target.totalAllocatedAdditions)
                {
                    return false;
                }
                if (target.totalAllocatedAdditions + target.totalFullSpace > target.maxCapacity)
                {
                    Release();
                    throw new Exception("Multi resource pool is over-allocated!");
                }
                var newAmount = amountToAdd;
                if (target.itemAmounts.TryGetValue(type, out float currentAmount))
                {
                    newAmount += currentAmount;
                }
                // add a new amount based on the Actual Amount, but deallocate the original
                //  Amount from the allocated additions sum
                target.itemAmounts[type] = newAmount;
                target.totalAllocatedAdditions -= Amount;
                return true;
            }

            protected override void DoRelease()
            {
                target.totalAllocatedAdditions = Math.Max(0, target.totalAllocatedAdditions - Amount);
            }
        }
        #endregion

        #region Subtraction Allocation

        public bool CanAllocateSubtraction(Resource type)
        {
            itemAmounts.TryGetValue(type, out float currentAmount);
            if (currentAmount <= 1e-5)
            {
                return false;
            }
            if (totalAllocatedSubtractions.TryGetValue(type, out float allocatedSubtraction))
            {
                return allocatedSubtraction < currentAmount;
            }
            return true;
        }

        public SubtractionAllocation TryAllocateSubtraction(Resource type, float amount = -1)
        {
            if (itemAmounts.TryGetValue(type, out float currentAmount))
            {
                totalAllocatedSubtractions.TryGetValue(type, out float allocatedSubtractions);
                if (amount < 0 || amount > (currentAmount - allocatedSubtractions))
                {
                    amount = currentAmount - allocatedSubtractions;
                }
                if (amount < 0)
                {
                    return null;
                }
                return new SubtractionAllocation(amount, type, this);
            }
            return null;
        }
        public class SubtractionAllocation : ResourceAllocation
        {
            private LimitedMultiResourcePool target;
            public Resource type { get; private set; }
            public SubtractionAllocation(
                float amount,
                Resource type,
                LimitedMultiResourcePool target) : base(amount, target)
            {
                this.target = target;
                this.type = type;
                if (target.totalAllocatedSubtractions.TryGetValue(type, out float allocatedSubs))
                {
                    target.totalAllocatedSubtractions[type] = amount + allocatedSubs;
                }
                else
                {
                    target.totalAllocatedSubtractions[type] = amount;
                }
            }

            protected override bool DoExecute(float actualAmount)
            {
                if (target.itemAmounts.TryGetValue(type, out float currentAmount))
                {
                    var amountToSub = Math.Min(Amount, actualAmount);
                    target.totalAllocatedSubtractions.TryGetValue(type, out float allocatedSubtractions);
                    if (allocatedSubtractions == default ||
                        amountToSub < 0 ||
                        amountToSub > allocatedSubtractions)
                    {
                        return false;
                    }
                    var newAmount = currentAmount - amountToSub;
                    if (newAmount < 0)
                    {
                        return false;
                    }
                    target.itemAmounts[type] = newAmount;
                    target.totalAllocatedSubtractions[type] -= Amount;
                    return true;
                }
                return false;
            }

            protected override void DoRelease()
            {
                if (target.totalAllocatedSubtractions.TryGetValue(type, out float allocatedSub))
                {
                    target.totalAllocatedSubtractions[type] = Math.Max(0, allocatedSub - Amount);
                }
            }
        }

        #endregion

        public IEnumerable<Resource> GetResourcesWithAllocatableSubtraction()
        {
            return itemAmounts.Where(kvp =>
            {
                totalAllocatedSubtractions.TryGetValue(kvp.Key, out float allocatedSubtraction);
                return kvp.Value > allocatedSubtraction;
            }).Select(x => x.Key);
        }

        public IEnumerable<Resource> GetResourcesWithAllocatableSpace()
        {
            if (!CanAllocateAddition())
            {
                return new Resource[0];
            }
            return Enum.GetValues(typeof(Resource)).Cast<Resource>();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Capacity: {maxCapacity}, remaining: {remainingCapacity}");
            SerializeResourceAmountsFromDictionary(itemAmounts, builder);
            builder.AppendLine("Allocated Subtractions: ");
            SerializeResourceAmountsFromDictionary(totalAllocatedSubtractions, builder);
            builder.AppendLine($"Allocated Additions: {totalAllocatedAdditions}");
            return builder.ToString();
        }

        private void SerializeResourceAmountsFromDictionary(IDictionary<Resource, float> amounts, StringBuilder builder)
        {
            foreach (var resource in amounts.Where(x => x.Value > 1e-5))
            {
                builder.AppendLine($"{Enum.GetName(typeof(Resource), resource.Key)}: {resource.Value:F1}");
            }
        }
    }
}
