using Unity.Collections;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    /// <summary>
    /// use the coordinate range to generate coordinates, and select out of the hash map based on those coordinates
    ///     and write any matching values from the hash map into the hash set writer
    /// </summary>
    public struct SelectBlockingTilesJob : IJobParallelFor
    {
        public UniversalCoordinateRange range;
        [ReadOnly] public NativeHashMap<UniversalCoordinate, int> coordinatesToTileIDs_input;
        [ReadOnly] public NativeHashSet<int> blockingTileIDs_input;
        [ReadOnly] public NativeHashSet<UniversalCoordinate> disabledCoordinates_input;
        [ReadOnly] public NativeHashSet<UniversalCoordinate> previewCoordinates_input;
        public NativeHashSet<UniversalCoordinate>.ParallelWriter HashSetWriter;

        public void Execute(int index)
        {
            var coordinate = range.AtIndex(index);
            if (IsBlocking(coordinate))
            {
                HashSetWriter.Add(coordinate);
            }
        }

        private bool IsBlocking(UniversalCoordinate coordinate)
        {
            if (disabledCoordinates_input.Contains(coordinate) || previewCoordinates_input.Contains(coordinate))
            {
                return true;
            }
            if (coordinatesToTileIDs_input.TryGetValue(coordinate, out var value))
            {
                if (blockingTileIDs_input.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
