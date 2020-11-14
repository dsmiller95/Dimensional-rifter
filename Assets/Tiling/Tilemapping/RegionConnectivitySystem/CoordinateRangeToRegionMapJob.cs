using Unity.Collections;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    public struct CoordinateRangeToRegionMapJob : IJob
    {
        [ReadOnly] public UniversalCoordinateRange coordinateRangeToIterate;
        [ReadOnly] public NativeHashSet<UniversalCoordinate> impassableTiles;

        /// <summary>
        /// populate this with the points from which to evaluate regions from. the
        ///     algorithm will ignore any blocked off regions which do not include these points
        /// these must be passable points, otherwise if they are on the boundary of what should be two regions
        ///     it will instead show only one region
        /// </summary>
        [ReadOnly] public NativeArray<UniversalCoordinate> seedPoints;

        /// <summary>
        /// hard limit of 32 regions, one for each bit in the data type
        /// </summary>
        public NativeHashMap<UniversalCoordinate, uint> outputRegionBitMasks;

        /// <summary>
        /// Used to store all points on the fringe of the current region iteration
        /// </summary>
        public NativeQueue<UniversalCoordinate> workingFringe;
        public NativeArray<UniversalCoordinate> workingNeighborCoordinatesSwapSpace;

        private int currentRegionIndex;

        /**
         * execution pattern:
         *  pick a seed point from the seed array
         *  If no region set for that seed, set with a new region ID, and add to the fringe.
         *      for each item dequed from the fringe:
         *          for each neighbor inside the coordinate range:
         *              assign its region ID to all neighbors
         *              if the neighbor is passable, and had no region ID orignally
         *                  add to the fringe
         */


        public void Execute()
        {
            currentRegionIndex = 0;
            for (int seedIndex = 0; seedIndex < seedPoints.Length; seedIndex++)
            {
                var seedPoint = seedPoints[seedIndex];
                if (!coordinateRangeToIterate.ContainsCoordinate(seedPoint))
                {
                    // the seed is on a different range, or out of bounds. ignore it
                    continue;
                }
                if (outputRegionBitMasks.TryGetValue(seedPoint, out var seedRegion))
                {
                    if (seedRegion != 0)
                    {
                        // this seed has already been visited, skip it
                        continue;
                    }
                }

                seedRegion = ((uint)1) << currentRegionIndex;
                currentRegionIndex++;
                outputRegionBitMasks[seedPoint] = seedRegion;
                workingFringe.Enqueue(seedPoint);

                BreadthFirstAssignFromQueue();
            }
        }

        private void BreadthFirstAssignFromQueue()
        {
            while (workingFringe.TryDequeue(out var nextNode))
            {
                var currentRegionBitMask = outputRegionBitMasks[nextNode];

                nextNode.SetNeighborsIntoSwapSpace(workingNeighborCoordinatesSwapSpace);
                var neighborCount = nextNode.NeighborCount();
                for (int i = 0; i < neighborCount; i++)
                {
                    var neighborCoordinate = workingNeighborCoordinatesSwapSpace[i];
                    if (!coordinateRangeToIterate.ContainsCoordinate(neighborCoordinate))
                    {
                        continue;
                    }

                    outputRegionBitMasks.TryGetValue(neighborCoordinate, out var originalNeighborRegionMask);
                    outputRegionBitMasks[neighborCoordinate] = originalNeighborRegionMask | currentRegionBitMask;

                    if (originalNeighborRegionMask == 0 && !impassableTiles.Contains(neighborCoordinate))
                    {
                        workingFringe.Enqueue(neighborCoordinate);
                    }
                }
            }
        }
    }
}
