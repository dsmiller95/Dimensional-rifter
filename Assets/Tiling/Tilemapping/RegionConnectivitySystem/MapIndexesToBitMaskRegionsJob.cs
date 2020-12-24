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
    public struct MapIndexesToBitMaskRegionsJob: IJobParallelFor
    {
        public UniversalCoordinateRange range;
        [ReadOnly] public NativeHashMap<UniversalCoordinate, int> regionIndexes_input;
        [ReadOnly] public NativeHashMap<int, int> regionRemappings_input;
        public NativeHashMap<UniversalCoordinate, uint>.ParallelWriter regionBitMasks_output;

        public void Execute(int index)
        {
            var coordinate = range.AtIndex(index);
            uint regionMask = 0;
            regionMask = AddRegionMask(coordinate, regionMask);
            for (int neighborIndex = 0; neighborIndex < coordinate.NeighborCount(); neighborIndex++)
            {
                var neighborCoord = coordinate.NeighborAtIndex(neighborIndex);
                regionMask = AddRegionMask(neighborCoord, regionMask);
            }
            regionBitMasks_output.TryAdd(coordinate, regionMask);
        }

        private uint AddRegionMask(UniversalCoordinate coordinate, uint currentMask)
        {
            if (regionIndexes_input.TryGetValue(coordinate, out int coordinateIndex) && coordinateIndex != -1)
            {
                if(regionRemappings_input.TryGetValue(coordinateIndex, out var remappedIndex))
                {
                    coordinateIndex = remappedIndex;
                }
                currentMask |= (uint)1 << coordinateIndex;
            }
            return currentMask;
        }
    }
}
