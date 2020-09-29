using Unity.Collections;
using Unity.Jobs;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    public struct CoordinateListingToGraphDataJob : IJob
    {
        public NativeArray<ConnectivityGraphNodeCoordinate> inputGraphCoordinates;
        public NativeHashMap<UniversalCoordinate, int> coordinateIndexes;

        public NativeArray<UniversalCoordinate> neighborCoordinateWorkSpace;

        public NativeArray<ConnectivityGraphNode> outputGraphNodes;
        public NativeArray<int> outputNeighborData;


        public void Execute()
        {
            for (var nodeIndex = 0; nodeIndex < inputGraphCoordinates.Length; nodeIndex++)
            {
                var coordinate = inputGraphCoordinates[nodeIndex];
                coordinateIndexes[coordinate.coordinate] = nodeIndex;
            }
            var currentIndexInNeighborArray = 0;
            for (var nodeIndex = 0; nodeIndex < inputGraphCoordinates.Length; nodeIndex++)
            {
                var coordinateNodeData = inputGraphCoordinates[nodeIndex];
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

                coord.SetNeighborsIntoSwapSpace(neighborCoordinateWorkSpace);
                var neighborCount = coord.NeighborCount();
                for (int i = 0; i < neighborCount; i++)
                {
                    var neighborCoordinate = neighborCoordinateWorkSpace[i];
                    if (coordinateIndexes.TryGetValue(neighborCoordinate, out int neighborIndexInMasterArray))
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
