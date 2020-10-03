using Assets.WorldObjects;
using System.Collections.Generic;
using Unity.Collections;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    public class ConnectivityGraphBuilder
    {
        private IList<ConnectivityGraphNodeCoordinate> allNodes = new List<ConnectivityGraphNodeCoordinate>();
        public GraphMembers allMembers = new GraphMembers();
        public int totalNeighbors = 0;

        public void AddNextNode(ConnectivityGraphNodeCoordinate nextNode, TileMapMember[] members)
        {
            totalNeighbors += nextNode.coordinate.NeighborCount();
            allNodes.Add(nextNode);
            if (members != null)
            {
                allMembers.allMembersByConnectivityID[CurrentNodeCount() - 1] = members;
            }
        }
        public int CurrentNodeCount()
        {
            return allNodes.Count;
        }

        public void BuildGraph(out NativeArray<ConnectivityGraphNodeCoordinate> graphNodes, Allocator allocator)
        {
            graphNodes = new NativeArray<ConnectivityGraphNodeCoordinate>(CurrentNodeCount(), allocator);

            for (var nodeIndex = 0; nodeIndex < allNodes.Count; nodeIndex++)
            {
                var nodeData = allNodes[nodeIndex];
                graphNodes[nodeIndex] = nodeData;
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
        public bool passable;
    }

    public class GraphMembers
    {
        public Dictionary<int, TileMapMember[]> allMembersByConnectivityID = new Dictionary<int, TileMapMember[]>();
    }

    public struct IndexInArrayLookup
    {
        /// <summary>
        /// Range in the array. inclusive on startIndex, exclusive on endIndex
        /// </summary>
        public int startIndex, endIndex;
    }
}
