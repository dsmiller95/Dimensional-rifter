using System;
using Unity.Collections;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    /// <summary>
    /// use the coordinate range to generate coordinates, and select out of the hash map based on those coordinates
    ///     and write any matching values from the hash map into the hash set writer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="J"></typeparam>
    public struct SelectFromCoordinateRangeJob<J> : IJobParallelFor
        where J : unmanaged, IEquatable<J>
    {
        public UniversalCoordinateRange range;
        [ReadOnly] public NativeHashMap<UniversalCoordinate, J> hashMapToFilter;
        [ReadOnly] public NativeHashSet<J> ValuesToSelectFor;
        public NativeHashSet<UniversalCoordinate>.ParallelWriter HashSetWriter;

        public void Execute(int index)
        {
            var coordinate = range.AtIndex(index);
            if(hashMapToFilter.TryGetValue(coordinate, out var value))
            {
                if (ValuesToSelectFor.Contains(value))
                {
                    HashSetWriter.Add(coordinate);
                }
            }
        }
    }
}
