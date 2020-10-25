using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    [CreateAssetMenu(fileName = "ConnectivitySystem", menuName = "TileMap/ConnectivitySystem", order = 1)]
    public class ConnectivitySystem : ScriptableObject
    {
        [Tooltip("Set to -1 to disable updates completely")]
        public float secondsPerConnectivityUpdate = 1;
        private float nextConnectivityUpdate = 5;

        private bool isJobRunning = false;
        private JobHandle currentlyRunningConnectionUpdate;
        private UniversalCoordinateSystemMembers memberDataFromRunningJob;

        private NativeArray<ConnectivityGraphNodeCoordinate> inputGraphNodes;
        private NativeHashSet<int> passableTileTypes;
        private NativeHashMap<UniversalCoordinate, int> coordinateIndexes;
        private NativeArray<UniversalCoordinate> tmpNeighborCoordinateSwapSpace;

        private NativeArray<ConnectivityGraphNode> resultGraphNodes;
        private NativeArray<int> neighborData;

        private NativeArray<ClassificationJobStatus> resultStatus;
        private NativeArray<int> finalRegionIndexAccess;
        private NativeArray<int> fullArrayIterationCount;
        private NativeArray<ClassifyIntoRegions.NodeVisitationStatus> nodeStatuses;


        /// <summary>
        /// Call this method once when loading the game up, to reset pending job status
        ///     and next connection update timing
        /// </summary>
        public void ResetState()
        {
            isJobRunning = false;
            nextConnectivityUpdate = 0;
        }

        public void StopEverything()
        {
            DisposeAllAndEnsureJobStopped();
        }

        /// <summary>
        /// Called every frame. It's up to this system to manage when if it does anything in response to this call
        /// </summary>
        public void TryUpdateConnectivity(Action<ConnectivityGraphBuilder> buildConnectivityGraph)
        {
            if (secondsPerConnectivityUpdate < 0)
            {
                return;
            }
            if (!isJobRunning)
            {
                if (nextConnectivityUpdate < Time.time)
                {
                    nextConnectivityUpdate = Time.time + secondsPerConnectivityUpdate;
                    isJobRunning = true;
                    CalculateConnectivityUpdate(buildConnectivityGraph);
                }
            }
            else
            {
                var jobHandle = currentlyRunningConnectionUpdate;
                if (jobHandle.IsCompleted && memberDataFromRunningJob != null)
                {
                    jobHandle.Complete();
                    var fullIterationNumbers = fullArrayIterationCount[0];
                    Debug.Log($"[Connectivity] Updated. Number of times iterated through whole array: {fullIterationNumbers}");

                    if (resultStatus[0] == ClassificationJobStatus.COMPLETED_TOO_MANY_REGIONS)
                    {
                        Debug.LogError("[Connectivity] Classification of regions failed: too many regions to fit in the bit mask");
                    }
                    else if (resultStatus[0] == ClassificationJobStatus.ERROR)
                    {
                        Debug.LogError("[Connectivity] Classification of regions failed: Very Big Loop detected");
                    }
                    else
                    {
                        Debug.Log($"[Connectivity] Found {finalRegionIndexAccess[0] + 1} seperate regions");
                        foreach (var (coordinate, members) in memberDataFromRunningJob.GetMembersByCoordinate())
                        {
                            if (coordinateIndexes.TryGetValue(coordinate, out var index))
                            {
                                var regionMask = resultGraphNodes[index].RegionMask;
                                foreach (var member in members)
                                {
                                    member.RegionBitMask = regionMask;
                                }
                            }
                        }
                    }

                    DisposeAllAndEnsureJobStopped();
                }
            }
        }

        private void DisposeAllAndEnsureJobStopped()
        {
            if (isJobRunning)
            {
                Debug.Log("disposing everything");
                memberDataFromRunningJob = null;
                inputGraphNodes.Dispose();
                passableTileTypes.Dispose();
                coordinateIndexes.Dispose();
                tmpNeighborCoordinateSwapSpace.Dispose();

                resultGraphNodes.Dispose();
                neighborData.Dispose();

                fullArrayIterationCount.Dispose();
                resultStatus.Dispose();
                finalRegionIndexAccess.Dispose();
                nodeStatuses.Dispose();
                isJobRunning = false;
            }
        }

        private void CalculateConnectivityUpdate(Action<ConnectivityGraphBuilder> buildConnectivityGraph)
        {
            var allocatorToUse = Allocator.Persistent;
            var connectionGraphBuilder = new ConnectivityGraphBuilder(allocatorToUse);
            buildConnectivityGraph(connectionGraphBuilder);

            connectionGraphBuilder.BuildGraph(
                out inputGraphNodes,
                out var readonlyTileTypeIDs,
                out passableTileTypes);

            memberDataFromRunningJob = connectionGraphBuilder.membersToReadFrom;

            coordinateIndexes = new NativeHashMap<UniversalCoordinate, int>(inputGraphNodes.Length, allocatorToUse);
            tmpNeighborCoordinateSwapSpace = new NativeArray<UniversalCoordinate>(UniversalCoordinate.MaxNeighborCount, allocatorToUse);

            resultGraphNodes = new NativeArray<ConnectivityGraphNode>(inputGraphNodes.Length, allocatorToUse);
            neighborData = new NativeArray<int>(connectionGraphBuilder.totalNeighbors, allocatorToUse);

            var neighborGraphingJob = new CoordinateListingToGraphDataJob
            {
                inputTileTypeIdByCoordinate = readonlyTileTypeIDs,
                inputPassableTileTypeIDs = passableTileTypes,
                inputAndWorkingGraphCoordinates = inputGraphNodes,

                workingNeighborCoordinates = tmpNeighborCoordinateSwapSpace,
                workingCoordinateIndexes = coordinateIndexes,

                outputGraphNodes = resultGraphNodes,
                outputNeighborData = neighborData
            };

            var neighborJobHandle = neighborGraphingJob.Schedule();

            resultStatus = new NativeArray<ClassificationJobStatus>(new[] { ClassificationJobStatus.RUNNING }, allocatorToUse);
            nodeStatuses = new NativeArray<ClassifyIntoRegions.NodeVisitationStatus>(
                resultGraphNodes.Length,
                allocatorToUse);

            finalRegionIndexAccess = new NativeArray<int>(1, allocatorToUse);
            fullArrayIterationCount = new NativeArray<int>(1, allocatorToUse);

            var classifyJob = new ClassifyIntoRegions
            {
                graphNodes = resultGraphNodes,
                intputNeighborData = neighborData,
                status = resultStatus,
                finalRegionIndex = finalRegionIndexAccess,
                fullArrayIterationCount = fullArrayIterationCount,
                NodeStatuses = nodeStatuses
            };
            currentlyRunningConnectionUpdate = classifyJob.Schedule(neighborJobHandle);
        }

        public enum ClassificationJobStatus
        {
            RUNNING,
            ERROR,
            COMPLETED_ALL_REGIONS,
            COMPLETED_TOO_MANY_REGIONS
        }

        struct ClassifyIntoRegions : IJob
        {
            public enum NodeVisitationStatus
            {
                /// <summary>
                /// default value for nodes which have not been touched by the algorithm at all
                /// </summary>
                NOT_VISITED = 0,
                /// <summary>
                /// used to mark any nodes which can be visited to update the current region
                /// </summary>
                PENDING,
                /// <summary>
                /// Used to mark nodes which should never be visited again: their neighbors have all
                ///     been marked pending if applicable, and it has applied its own region mask
                ///     to all neighbors
                /// </summary>
                COMPLETE
            }


            public NativeArray<ConnectivityGraphNode> graphNodes;
            public NativeArray<int> intputNeighborData;

            public NativeArray<NodeVisitationStatus> NodeStatuses;

            public NativeArray<ClassificationJobStatus> status;
            public NativeArray<int> finalRegionIndex;
            public NativeArray<int> fullArrayIterationCount;

            private int currentRegionIndex;

            public void Execute()
            {
                fullArrayIterationCount[0] = 0;
                DoExecute();
                finalRegionIndex[0] = currentRegionIndex;
            }

            private void DoExecute()
            {
                var currentIndex = 0;
                currentRegionIndex = -1;
                for (; currentIndex < graphNodes.Length; currentIndex++)
                {
                    var currentNodeStatus = NodeStatuses[currentIndex];
                    if (currentNodeStatus == NodeVisitationStatus.COMPLETE)
                    {
                        continue;
                    }
                    var currentNode = graphNodes[currentIndex];
                    if (!currentNode.Passable)
                    {
                        // this node is impassible, it's impossible to begin walking a region from it
                        //  marks as Complete and move on.
                        NodeStatuses[currentIndex] = NodeVisitationStatus.COMPLETE;
                        continue;
                    }

                    currentRegionIndex++;
                    if (currentRegionIndex >= 64)
                    {
                        status[0] = ClassificationJobStatus.COMPLETED_TOO_MANY_REGIONS;
                        return;
                    }

                    AssignAllPossibleToRegion(currentIndex, ((ulong)1) << currentRegionIndex);
                    if (status[0] == ClassificationJobStatus.ERROR)
                    {
                        return;
                    }
                }
                status[0] = ClassificationJobStatus.COMPLETED_ALL_REGIONS;
            }

            /// <summary>
            /// take the region bit mask and apply it to all nodes adjacent to the starting index,
            ///  if possible. before entering this method ensure that the startingNodeIndex is not
            ///  Complete, and that it is Passable
            /// </summary>
            /// <param name="startingNodeIndex"></param>
            /// <param name="regionBitMask"></param>
            private void AssignAllPossibleToRegion(int startingNodeIndex, ulong regionBitMask)
            {
                AssignRegionsStartingAtNode(startingNodeIndex, regionBitMask);

                bool foundAtLeastOnePending;
                var infiniteProtector = 10000;
                do
                {
                    foundAtLeastOnePending = false;
                    for (int nodeIndex = 0; nodeIndex < NodeStatuses.Length; nodeIndex++)
                    {
                        var status = NodeStatuses[nodeIndex];
                        if (status == NodeVisitationStatus.PENDING)
                        {
                            foundAtLeastOnePending = true;
                            AssignRegionsStartingAtNode(nodeIndex, regionBitMask);
                        }
                    }
                    fullArrayIterationCount[0] = fullArrayIterationCount[0] + 1;
                    infiniteProtector--;
                    if (infiniteProtector <= 0)
                    {
                        status[0] = ClassificationJobStatus.ERROR;
                        return;
                    }
                } while (foundAtLeastOnePending && status[0] != ClassificationJobStatus.ERROR);
            }

            /// <summary>
            /// walk through neighbors, using a pseudo-depth-first search, without using a stack.
            ///     Always mark every neighbor with the region mask. only visit a neighbor if it is
            ///     both passable and not completed.
            /// Each node processed will be marked as completed, after all applicable neighbors are marked as pending
            /// Each node will continue on to only one of its neighbors. this means that the algo will not process
            ///     every node in a given region, instead relying on marking the fringes with Pending so they can be
            ///     found later and used as a new entry point
            /// </summary>
            /// <param name="currentNodeIndex"></param>
            /// <param name="regionBitMask"></param>
            /// <returns>true if any updates were made by this method</returns>
            private bool AssignRegionsStartingAtNode(int currentNodeIndex, ulong regionBitMask)
            {
                if (NodeStatuses[currentNodeIndex] == NodeVisitationStatus.COMPLETE)
                {
                    return false;
                }
                var currentNode = graphNodes[currentNodeIndex];
                currentNode.RegionMask |= regionBitMask;
                graphNodes[currentNodeIndex] = currentNode;

                var walkLength = 0;

                var infiniteProtector = 100000;
                while (currentNodeIndex >= 0)
                {
                    ConnectivityGraphNode nextNode = default;
                    int nextNodeIndex = -1;
                    for (var indexInNeighbors = currentNode.NeighborLookup.startIndex;
                        indexInNeighbors < currentNode.NeighborLookup.endIndex;
                        indexInNeighbors++)
                    {
                        var neighborNodeIndex = intputNeighborData[indexInNeighbors];
                        var neighborNode = graphNodes[neighborNodeIndex];

                        if ((neighborNode.RegionMask & regionBitMask) != 0)
                        {
                            // this node has already been masked by the same bit mask as this node.
                            //  don't waste time doing anything else
                            continue;
                        }

                        neighborNode.RegionMask |= regionBitMask;
                        graphNodes[neighborNodeIndex] = neighborNode;

                        var neighborNodeStatus = NodeStatuses[neighborNodeIndex];
                        if (neighborNodeStatus != NodeVisitationStatus.COMPLETE && neighborNode.Passable)
                        {
                            NodeStatuses[neighborNodeIndex] = NodeVisitationStatus.PENDING;

                            nextNode = neighborNode;
                            nextNodeIndex = neighborNodeIndex;
                        }
                    }
                    NodeStatuses[currentNodeIndex] = NodeVisitationStatus.COMPLETE;
                    graphNodes[currentNodeIndex] = currentNode;

                    currentNodeIndex = nextNodeIndex;
                    currentNode = nextNode;
                    walkLength++;

                    infiniteProtector--;
                    if (infiniteProtector <= 0)
                    {
                        status[0] = ClassificationJobStatus.ERROR;
                        return true;
                    }
                }
                return true;
            }
        }
    }
}
