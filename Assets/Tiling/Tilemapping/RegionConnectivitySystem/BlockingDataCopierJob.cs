using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Wall.DOTS;
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
    public struct BlockingDataCopier : IJobParallelFor
    {
        [ReadOnly] public NativeArray<TileBlockingComponent> isBlocking;
        [ReadOnly] public NativeArray<UniversalCoordinatePositionComponent> positions;
        public NativeHashSet<UniversalCoordinate>.ParallelWriter HashSetWriter;

        public void Execute(int index)
        {
            if (isBlocking[index].CurrentlyBlocking)
            {
                var value = positions[index];
                HashSetWriter.Add(value.Value);
            }
        }
    }
}
