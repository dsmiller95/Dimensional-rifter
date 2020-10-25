using Unity.Collections;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    public struct CoordinateListingToGraphDataJob : IJob
    {
        [ReadOnly]
        public NativeHashMap<UniversalCoordinate, int> inputTileTypeIdByCoordinate;

        [ReadOnly]
        public NativeHashSet<int> inputPassableTileTypeIDs;

        public NativeArray<ConnectivityGraphNodeCoordinate> inputAndWorkingGraphCoordinates;

        public NativeHashMap<UniversalCoordinate, int> workingCoordinateIndexes;
        public NativeArray<UniversalCoordinate> workingNeighborCoordinates;

        public NativeArray<ConnectivityGraphNode> outputGraphNodes;
        public NativeArray<int> outputNeighborData;

        public void Execute()
        {
            AssignCoordinateIndexesAndPassabilityBasedOnTileTypes();
            AssignAllNeighborIndexesAndLookups();
        }

        private void AssignCoordinateIndexesAndPassabilityBasedOnTileTypes()
        {
            for (var nodeIndex = 0; nodeIndex < inputAndWorkingGraphCoordinates.Length; nodeIndex++)
            {
                var coordinate = inputAndWorkingGraphCoordinates[nodeIndex];
                var typeID = inputTileTypeIdByCoordinate[coordinate.coordinate];
                var isPassable = inputPassableTileTypeIDs.Contains(typeID);
                coordinate.passable = isPassable;
                inputAndWorkingGraphCoordinates[nodeIndex] = coordinate;
                workingCoordinateIndexes[coordinate.coordinate] = nodeIndex;
            }
        }

        private void AssignAllNeighborIndexesAndLookups()
        {
            var currentIndexInNeighborArray = 0;
            for (var nodeIndex = 0; nodeIndex < inputAndWorkingGraphCoordinates.Length; nodeIndex++)
            {
                var coordinateNodeData = inputAndWorkingGraphCoordinates[nodeIndex];
                var coord = coordinateNodeData.coordinate;

                var newGraphNode = new ConnectivityGraphNode
                {
                    NeighborLookup = new IndexInArrayLookup
                    {
                        startIndex = currentIndexInNeighborArray
                    },
                    Passable = coordinateNodeData.passable,
                    RegionMask = 0
                };

                coord.SetNeighborsIntoSwapSpace(workingNeighborCoordinates);
                var neighborCount = coord.NeighborCount();
                for (int i = 0; i < neighborCount; i++)
                {
                    var neighborCoordinate = workingNeighborCoordinates[i];
                    if (workingCoordinateIndexes.TryGetValue(neighborCoordinate, out int neighborIndexInMasterArray))
                    {
                        outputNeighborData[currentIndexInNeighborArray] = neighborIndexInMasterArray;
                        currentIndexInNeighborArray++;
                    }
                }
                newGraphNode.NeighborLookup.endIndex = currentIndexInNeighborArray;

                outputGraphNodes[nodeIndex] = newGraphNode;
            }
        }
    }
}
