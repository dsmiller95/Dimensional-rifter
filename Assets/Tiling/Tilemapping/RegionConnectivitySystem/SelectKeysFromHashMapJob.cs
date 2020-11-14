using System;
using Unity.Collections;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    /// <summary>
    /// Iterate through hashMapToFilter. for every value which is contained in the ValuesToSelectFor set,
    ///     add the Key to the HashSetWriter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="J"></typeparam>
    public struct SelectKeysfromHashMapJob<T, J> : IJobParallelFor
        where T : unmanaged, IEquatable<T>
        where J : unmanaged, IEquatable<J>
    {
        [ReadOnly] public NativeKeyValueArrays<T, J> hashMapToFilter;
        [ReadOnly] public NativeHashSet<J> ValuesToSelectFor;
        public NativeHashSet<T>.ParallelWriter HashSetWriter;

        public void Execute(int index)
        {
            var value = hashMapToFilter.Values[index];
            if (ValuesToSelectFor.Contains(value))
            {
                var key = hashMapToFilter.Keys[index];
                HashSetWriter.Add(key);
            }
        }
    }
}
