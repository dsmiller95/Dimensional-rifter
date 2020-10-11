using Assets.WorldObjects.SaveObjects;
using System;

namespace Assets.Scripts.ResourceManagement
{
    [Serializable]
    public class LimitedResourcePoolSaveObject
    {
        public float currentAmount;
        public float maxCapacity;
    }

    public class LimitedResourcePool
    {
        private float currentAmount;
        private float maxCapacity;

        private float totalAllocatedAdditions;
        private float totalAllocatedSubtractions;


        public LimitedResourcePool(float capacity, float initialAmount)
        {
            currentAmount = initialAmount;
            totalAllocatedAdditions = 0f;
            maxCapacity = capacity;
        }

        public LimitedResourcePool(LimitedResourcePoolSaveObject saveObject): this(saveObject.maxCapacity, saveObject.currentAmount)
        {
        }
        public LimitedResourcePoolSaveObject GetSaveObject()
        {
            return new LimitedResourcePoolSaveObject()
            {
                currentAmount = currentAmount,
                maxCapacity = maxCapacity
            };
        }

        public bool CanAllocateAddition()
        {
            return currentAmount + totalAllocatedAdditions < maxCapacity;
        }
        public AdditionAllocation TryAllocateAddition(float amount)
        {
            if(amount + totalAllocatedAdditions + currentAmount > maxCapacity)
            {
                amount = maxCapacity - (totalAllocatedAdditions + currentAmount);
            }
            if(amount < 0)
            {
                return null;
            }
            return new AdditionAllocation(amount, this);
        }

        public bool CanAllocateSubtraction()
        {
            return (currentAmount - totalAllocatedSubtractions) > 1e-5;
        }
        public SubtractionAllocation TryAllocateSubtraction(float amount = -1)
        {
            if (amount < 0 || amount > (currentAmount - totalAllocatedSubtractions))
            {
                amount = currentAmount - totalAllocatedSubtractions;
            }
            if(amount <= 0)
            {
                return null;
            }
            return new SubtractionAllocation(amount, this);
        }

        public class AdditionAllocation : ResourceAllocation
        {
            private LimitedResourcePool target;
            public AdditionAllocation(float amount, LimitedResourcePool target) : base(amount, target)
            {
                this.target = target;
                target.totalAllocatedAdditions += amount;
            }

            protected override bool DoExecute(float actualAmount)
            {
                var amountToAdd = Math.Min(Amount, actualAmount);
                if (amountToAdd < 0 || amountToAdd > target.totalAllocatedAdditions)
                {
                    return false;
                }

                var newAmount = target.currentAmount + amountToAdd;
                if (newAmount > target.maxCapacity)
                {
                    this.Release();
                    throw new Exception("resource pool additions are over-allocated!");
                }
                // add a new amount based on the Actual Amount, but deallocate the original
                //  Amount from the allocated additions sum
                target.currentAmount = newAmount;
                target.totalAllocatedAdditions -= Amount;
                return true;
            }

            protected override void DoRelease()
            {
                target.totalAllocatedAdditions = Math.Max(0, target.totalAllocatedAdditions - Amount);
            }
        }

        public class SubtractionAllocation : ResourceAllocation
        {
            private LimitedResourcePool target;
            public SubtractionAllocation(float amount, LimitedResourcePool target) : base(amount, target)
            {
                this.target = target;
                target.totalAllocatedSubtractions += amount;
            }

            protected override bool DoExecute(float actualAmount)
            {
                var amountToSub = Math.Min(Amount, actualAmount);
                if (amountToSub < 0 || amountToSub > target.totalAllocatedSubtractions)
                {
                    return false;
                }
                var newAmount = target.currentAmount - amountToSub;
                if (newAmount < 0)
                {
                    this.Release();
                    throw new Exception("resource pool subtractions are over-allocated!");
                }
                target.currentAmount = newAmount;
                target.totalAllocatedSubtractions -= Amount;
                return true;
            }

            protected override void DoRelease()
            {
                target.totalAllocatedSubtractions = Math.Max(0, target.totalAllocatedSubtractions - Amount);
            }
        }
    }
}
