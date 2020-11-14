using System.Linq;
using Unity.Collections;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    public class ConnectivityGraphBuilder
    {
        public int totalNeighbors = 0;

        private NativeArray<ConnectivityGraphNodeCoordinate> nodeArray;
        private Allocator allocator;
        private int currentNodeIndex = 0;

        public UniversalCoordinateSystemMembers membersToReadFrom;

        public ConnectivityGraphBuilder(Allocator allocator)
        {
            this.allocator = allocator;
        }

        public void ReadFromTileDataIn(UniversalCoordinateSystemMembers tileMemberDataHolder)
        {
            membersToReadFrom = tileMemberDataHolder;
        }

        public void InitNodeBuilderArrayWithCapacity(int maxNodeSpace)
        {
            nodeArray = new NativeArray<ConnectivityGraphNodeCoordinate>(maxNodeSpace, allocator);
        }

        public void NextNode(ConnectivityGraphNodeCoordinate node, int possibleNeighbors)
        {
            nodeArray[currentNodeIndex] = node;
            totalNeighbors += possibleNeighbors;
            currentNodeIndex++;
        }

        public void BuildGraph(
            out NativeArray<ConnectivityGraphNodeCoordinate> graphNodes,
            out NativeHashMap<UniversalCoordinate, int> tileTypeIDs,
            out NativeHashSet<int> passableIDs)
        {
            graphNodes = nodeArray;

            tileTypeIDs = membersToReadFrom.GetTileTypesByCoordinateReadonlyCollection();

            // TODO: figure out how to let the connectivity system know about walls and other blocking members
            var passableSet = membersToReadFrom.GetTileInfoByTypeIndex()
                .Select((x, i) => new
                {
                    passable = x.isPassable,
                    index = i
                })
                .Where(x => x.passable)
                .Select(x => x.index)
                .ToArray();
            passableIDs = new NativeHashSet<int>(passableSet.Count(), allocator);
            foreach (var id in passableSet)
            {
                passableIDs.Add(id);
            }
        }
    }

    public struct ConnectivityGraphNode
    {
        public IndexInArrayLookup NeighborLookup;
        /// <summary>
        /// ID of the region this node belongs to. negative value indicates unknown region
        /// uses bit-masking to apply multiple IDs
        /// </summary>
        public ulong RegionMask;
        public bool Passable;
    }

    public struct ConnectivityGraphNodeCoordinate
    {
        public UniversalCoordinate coordinate;
        /// <summary>
        /// set in the job based on tile config data
        /// </summary>
        public bool passable;
    }

    public struct IndexInArrayLookup
    {
        /// <summary>
        /// Range in the array. inclusive on startIndex, exclusive on endIndex
        /// </summary>
        public int startIndex, endIndex;
    }
}
