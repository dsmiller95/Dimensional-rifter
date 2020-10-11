namespace Assets.Scripts.ResourceManagement
{
    public abstract class ResourceAllocation
    {
        public float Amount;
        protected bool isReleased;

        private object targetObject;

        public ResourceAllocation(float amount, object target)
        {
            Amount = amount;
            isReleased = false;
            targetObject = target;
        }

        public bool IsTarget(object possibleTarget)
        {
            return targetObject == possibleTarget;
        }

        public bool Execute()
        {
            return this.Execute(this.Amount);
        }

        /// <summary>
        /// Attempts to execute the allocation by applying its claim to the base pool.
        ///     If this fails it means that resources have been mis-allocated and mishandled
        /// Will always release this allocation, regardless of if the execution was successfull or not
        /// </summary>
        /// <returns></returns>
        public bool Execute(float actualAmount)
        {
            if (isReleased)
            {
                return false;
            }
            var result = DoExecute(actualAmount);
            if (result)
            {
                // transfer was executed, cannot be executed again
                isReleased = true;
            }
            else
            {
                // transfer was not executed, must manually release
                Release();
            }
            return result;
        }

        public void ReduceClaim(float smallerClaim)
        {
            if (Amount <= smallerClaim || smallerClaim < 0)
            {
                return;
            }
            TryReduceClaimToSmaller(smallerClaim);
            Amount = smallerClaim;
        }

        protected abstract bool TryReduceClaimToSmaller(float smallerClaim);

        /// <summary>
        /// return true if the allocation was executed at all, false otherwise
        ///     If returning true, the resource must handle its own resource release process
        /// </summary>
        /// <returns></returns>
        protected abstract bool DoExecute(float actualAmount);

        public void Release()
        {
            if (isReleased)
            {
                return;
            }
            DoRelease();
            isReleased = true;
        }
        protected abstract void DoRelease();
    }
}
