using System;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    /// <summary>
    /// Used to manage data which is generated as part of a long-running job,
    ///     but also needs to be cached for other systems to access while the job is running
    /// </summary>
    public class NativeDisposableHotSwap<T> : IDisposable where T : struct, IDisposable
    {
        private bool CurrentActiveRegionClassification;
        private T? regionClassificationFalse = null;
        private T? regionClassificationTrue = null;

        private bool HasPending;

        public T? ActiveData => CurrentActiveRegionClassification ? regionClassificationTrue : regionClassificationFalse;

        /// <summary>
        /// hot swaps to the pending data. Will only do anything if AssignPending was called at some point
        ///     since the last call to this method
        /// </summary>
        public void HotSwapToPending()
        {
            if (HasPending)
            {
                CurrentActiveRegionClassification = !CurrentActiveRegionClassification;
                HasPending = false;
            }
        }
        /// <summary>
        /// assigns the internal pending native collection, and disposes the existing one in the currently pending slot
        ///     if it exists
        /// </summary>
        /// <param name="pendingRegion"></param>
        public void AssignPending(T pendingRegion)
        {
            if (HasPending)
            {
                Debug.LogWarning("Assigning Pending value without a hotswap happening. A previously pending value must be disposed");
            }
            HasPending = true;
            if (CurrentActiveRegionClassification)
            {
                if (regionClassificationFalse.HasValue)
                {
                    regionClassificationFalse.Value.Dispose();
                }
                regionClassificationFalse = pendingRegion;
            }
            else
            {
                if (regionClassificationTrue.HasValue)
                {
                    regionClassificationTrue.Value.Dispose();
                }
                regionClassificationTrue = pendingRegion;
            }
        }

        public void Dispose()
        {
            if (regionClassificationTrue.HasValue)
            {
                regionClassificationTrue.Value.Dispose();
            }
            if (regionClassificationFalse.HasValue)
            {
                regionClassificationFalse.Value.Dispose();
            }
        }
    }
}
