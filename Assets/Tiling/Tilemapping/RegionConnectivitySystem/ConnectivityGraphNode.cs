using Assets.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    public class ConnectivityGraphBuilder
    {
        private IList<ConnectivityGraphNodeBuilder> nodeBuilders = new List<ConnectivityGraphNodeBuilder>();

        private int totalNeighbors = 0;

        public void AddNextNode(ConnectivityGraphNodeBuilder nextNode)
        {
            nextNode.neighborBeginIndex = totalNeighbors;
            totalNeighbors += nextNode.neighborIndexes.Count();
            nextNode.neighborEndIndex = totalNeighbors;
            nodeBuilders.Add(nextNode);
        }
        public int CurrentNodeCount()
        {
            return nodeBuilders.Count;
        }

        public GraphMembers BuildGraph(out NativeArray<ConnectivityGraphNode> graphNodes, out NativeArray<int> neighborData, Allocator allocator)
        {
            neighborData = new NativeArray<int>(totalNeighbors, allocator);
            graphNodes = new NativeArray<ConnectivityGraphNode>(nodeBuilders.Count, allocator);

            var graphMemberNodes = new GraphMembers();

            for (var nodeIndex = 0; nodeIndex < nodeBuilders.Count; nodeIndex++)
            {
                var nodeBuilder = nodeBuilders[nodeIndex];
                var graphNode = new ConnectivityGraphNode
                {
                    RegionMask = 0,
                    Passable = nodeBuilder.isPassable,
                    NeighborLookup = new IndexInArrayLookup
                    {
                        startIndex = nodeBuilder.neighborBeginIndex,
                        endIndex = nodeBuilder.neighborEndIndex
                    }
                };
                graphNodes[nodeIndex] = graphNode;
                for (var neighborIndex = 0; neighborIndex < nodeBuilder.neighborIndexes.Count(); neighborIndex++)
                {
                    var indexInNeighborData = neighborIndex + graphNode.NeighborLookup.startIndex;
                    neighborData[indexInNeighborData] = nodeBuilder.neighborIndexes[neighborIndex];
                }

                if (nodeBuilder.membersHere.Count > 0)
                {
                    graphMemberNodes.allMembersByConnectivityID[nodeIndex] = nodeBuilder.membersHere.ToArray();
                }
            }

            return graphMemberNodes;
        }
    }

    public class ConnectivityGraphNodeBuilder
    {
        public ConnectivityGraphNodeBuilder(int neighborCount)
        {
            neighborIndexes = new List<int>(neighborCount);
        }
        public ConnectivityGraphNodeBuilder(IEnumerable<int> neighbors)
        {
            neighborIndexes = neighbors.ToList();
        }
        public IList<int> neighborIndexes;
        public bool isPassable;
        public IList<TileMapMember> membersHere;

        public int neighborBeginIndex;
        public int neighborEndIndex;
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
