using Unity.Collections;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    public struct CoordinateRangeToRegionMapJob : IJob
    {
        [ReadOnly] public UniversalCoordinateRange coordinateRangeToIterate_input;
        [ReadOnly] public NativeHashSet<UniversalCoordinate> impassableTiles_input;

        /// <summary>
        /// populate this with the points from which to evaluate regions from. the
        ///     algorithm will ignore any blocked off regions which do not include these points
        /// these must be passable points, otherwise if they are on the boundary of what should be two regions
        ///     it will instead show only one region
        /// </summary>
        [ReadOnly] public NativeArray<UniversalCoordinate> seedPoints_input;

        /// <summary>
        /// hard limit of 32 regions, one for each bit in the data type
        /// </summary>
        public NativeHashMap<UniversalCoordinate, uint> regionBitMasks_output;
        public NativeList<AllocatedRegion> allRegions_output;
        public NativeArray<int> regionCounter_working;

        /// <summary>
        /// Used to store all points on the fringe of the current region iteration
        /// </summary>
        public NativeQueue<UniversalCoordinate> fringe_working;
        [DeallocateOnJobCompletion] public NativeArray<UniversalCoordinate> neighborCoordinatesSwapSpace_working;

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
            for (int seedIndex = 0; seedIndex < seedPoints_input.Length; seedIndex++)
            {
                var seedPoint = seedPoints_input[seedIndex];
                if (!coordinateRangeToIterate_input.ContainsCoordinate(seedPoint))
                {
                    // the seed is on a different range, or out of bounds. ignore it
                    continue;
                }
                if (regionBitMasks_output.TryGetValue(seedPoint, out var seedRegion))
                {
                    if (seedRegion != 0)
                    {
                        // this seed has already been visited, skip it
                        continue;
                    }
                }

                seedRegion = ((uint)1) << regionCounter_working[0];
                allRegions_output.Add(new AllocatedRegion
                {
                    regionIndex = regionCounter_working[0],
                    planeIndex = seedPoint.CoordinatePlaneID
                });
                regionCounter_working[0]++;
                regionBitMasks_output[seedPoint] = seedRegion;
                fringe_working.Enqueue(seedPoint);

                BreadthFirstAssignFromQueue();
            }
        }

        private void BreadthFirstAssignFromQueue()
        {
            while (fringe_working.TryDequeue(out var nextNode))
            {
                var currentRegionBitMask = regionBitMasks_output[nextNode];

                nextNode.SetNeighborsIntoSwapSpace(neighborCoordinatesSwapSpace_working);
                var neighborCount = nextNode.NeighborCount();
                for (int i = 0; i < neighborCount; i++)
                {
                    var neighborCoordinate = neighborCoordinatesSwapSpace_working[i];
                    if (!coordinateRangeToIterate_input.ContainsCoordinate(neighborCoordinate))
                    {
                        continue;
                    }

                    regionBitMasks_output.TryGetValue(neighborCoordinate, out var originalNeighborRegionMask);
                    regionBitMasks_output[neighborCoordinate] = originalNeighborRegionMask | currentRegionBitMask;

                    if (originalNeighborRegionMask == 0 && !impassableTiles_input.Contains(neighborCoordinate))
                    {
                        fringe_working.Enqueue(neighborCoordinate);
                    }
                }
            }
        }
    }
}
