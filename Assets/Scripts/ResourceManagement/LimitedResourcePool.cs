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
        public float CurrentAmount { get; private set; }
        private float maxCapacity;

        private float totalAllocatedAdditions;
        private float totalAllocatedSubtractions;


        public LimitedResourcePool(float capacity, float initialAmount)
        {
            CurrentAmount = initialAmount;
            totalAllocatedAdditions = 0f;
            maxCapacity = capacity;
        }

        public LimitedResourcePool(LimitedResourcePoolSaveObject saveObject) : this(saveObject.maxCapacity, saveObject.currentAmount)
        {
        }
        public LimitedResourcePoolSaveObject GetSaveObject()
        {
            return new LimitedResourcePoolSaveObject()
            {
                currentAmount = CurrentAmount,
                maxCapacity = maxCapacity
            };
        }

        public override string ToString()
        {
            return $"Amount: {CurrentAmount} Capacity: {maxCapacity}\n" +
                $"Adds: {totalAllocatedAdditions} Subs: {totalAllocatedSubtractions}";
        }

        public bool CanAllocateAddition()
        {
            return CurrentAmount + totalAllocatedAdditions < maxCapacity;
        }
        public AdditionAllocation TryAllocateAddition(float amount)
        {
            if (amount + totalAllocatedAdditions + CurrentAmount > maxCapacity)
            {
                amount = maxCapacity - (totalAllocatedAdditions + CurrentAmount);
            }
            if (amount < 0)
            {
                return null;
            }
            return new AdditionAllocation(amount, this);
        }

        public bool CanAllocateSubtraction()
        {
            return (CurrentAmount - totalAllocatedSubtractions) > 1e-5;
        }
        public SubtractionAllocation TryAllocateSubtraction(float amount = -1)
        {
            if (amount < 0 || amount > (CurrentAmount - totalAllocatedSubtractions))
            {
                amount = CurrentAmount - totalAllocatedSubtractions;
            }
            if (amount <= 0)
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

                var newAmount = target.CurrentAmount + amountToAdd;
                if (newAmount > target.maxCapacity)
                {
                    Release();
                    throw new Exception("resource pool additions are over-allocated!");
                }
                // add a new amount based on the Actual Amount, but deallocate the original
                //  Amount from the allocated additions sum
                target.CurrentAmount = newAmount;
                target.totalAllocatedAdditions -= Amount;
                return true;
            }
            protected override bool TryReduceClaimToSmaller(float smallerClaim)
            {
                var differenceInSize = Amount - smallerClaim;
                target.totalAllocatedAdditions -= differenceInSize;
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
                var newAmount = target.CurrentAmount - amountToSub;
                if (newAmount < 0)
                {
                    Release();
                    throw new Exception("resource pool subtractions are over-allocated!");
                }
                target.CurrentAmount = newAmount;
                target.totalAllocatedSubtractions -= Amount;
                return true;
            }
            protected override bool TryReduceClaimToSmaller(float smallerClaim)
            {
                var differenceInSize = Amount - smallerClaim;
                target.totalAllocatedSubtractions -= differenceInSize;
                return true;
            }

            protected override void DoRelease()
            {
                target.totalAllocatedSubtractions = Math.Max(0, target.totalAllocatedSubtractions - Amount);
            }
        }
    }
}
