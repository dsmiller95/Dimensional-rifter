using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.RegionConnectivitySystem
{
    [CreateAssetMenu(fileName = "ConnectivitySystem", menuName = "TileMap/ConnectivitySystem", order = 1)]
    public class ConnectivitySystem : ScriptableObject
    {
        //public <ConnectivityGraphNode> CurrentConnectivity;

        public float secondsPerConnectivityUpdate = 1;
        private float nextConnectivityUpdate = 5;

        private bool isJobRunning = false;
        private JobHandle currentlyRunningConnectionUpdate;
        private GraphMembers memberDataFromRunningJob;

        private NativeArray<ConnectivityGraphNode> resultGraphNodes;
        private NativeArray<int> neighborData;

        private NativeArray<ClassificationJobStatus> resultStatus;
        private NativeArray<int> finalRegionIndexAccess;
        private NativeArray<ClassifyIntoRegions.NodeVisitationStatus> nodeStatuses;


        /// <summary>
        /// Call this method once when loading the game up, to reset pending job status
        ///     and next connection update timing
        /// </summary>
        public void ResetState()
        {
            isJobRunning = false;
            nextConnectivityUpdate = 5;
        }

        public void StopEverything()
        {
            DisposeAllAndEnsureJobStopped();
        }

        /// <summary>
        /// Called every frame. It's up to this system to manage when if it does anything in response to this call
        /// </summary>
        public void TryUpdateConnectivity(Func<IEnumerable<TileMapRegionNoCoordinateType>> getRegions)
        {
            if (!isJobRunning)
            {
                if (nextConnectivityUpdate < Time.time)
                {
                    nextConnectivityUpdate = Time.time + secondsPerConnectivityUpdate;
                    Debug.Log("Updating connectivity");
                    isJobRunning = true;
                    CalculateConnectivityUpdate(getRegions());
                }
            }
            else
            {
                var jobHandle = currentlyRunningConnectionUpdate;
                if (jobHandle.IsCompleted && memberDataFromRunningJob != null)
                {
                    Debug.Log("connectivity update success");
                    jobHandle.Complete();

                    if (resultStatus[0] == ClassificationJobStatus.COMPLETED_TOO_MANY_REGIONS)
                    {
                        Debug.LogError("Classification of regions failed: too many regions to fit in the bit mask");
                    }
                    else if (resultStatus[0] == ClassificationJobStatus.ERROR)
                    {
                        Debug.LogError("Classification of regions failed: Very Big Loop detected");
                    }
                    else
                    {
                        Debug.Log($"Found {finalRegionIndexAccess[0] + 1} seperate regions");

                        foreach (var kvp in memberDataFromRunningJob.allMembersByConnectivityID)
                        {
                            var regionMask = resultGraphNodes[kvp.Key].RegionMask;
                            foreach (var member in kvp.Value)
                            {
                                member.RegionBitMask = regionMask;
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
                isJobRunning = false;
                memberDataFromRunningJob = null;
                resultGraphNodes.Dispose();
                neighborData.Dispose();

                resultStatus.Dispose();
                finalRegionIndexAccess.Dispose();
                nodeStatuses.Dispose();
            }

        }

        private void CalculateConnectivityUpdate(IEnumerable<TileMapRegionNoCoordinateType> regions)
        {
            var connectionGraphBuilder = new ConnectivityGraphBuilder();

            foreach (var region in regions)
            {
                region.AddConnectivityAndMemberData(connectionGraphBuilder);
            }
            var allocatorToUse = Allocator.Persistent;
            memberDataFromRunningJob = connectionGraphBuilder.BuildGraph(
                out resultGraphNodes,
                out neighborData,
                allocatorToUse);

            resultStatus = new NativeArray<ClassificationJobStatus>(new[] { ClassificationJobStatus.RUNNING }, allocatorToUse);
            finalRegionIndexAccess = new NativeArray<int>(1, allocatorToUse);
            nodeStatuses = new NativeArray<ClassifyIntoRegions.NodeVisitationStatus>(
                resultGraphNodes.Length,
                allocatorToUse);

            var classifyJob = new ClassifyIntoRegions
            {
                graphNodes = resultGraphNodes,
                neighborData = neighborData,
                status = resultStatus,
                finalRegionIndex = finalRegionIndexAccess,
                NodeStatuses = nodeStatuses
            };
            currentlyRunningConnectionUpdate = classifyJob.Schedule();
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
            public NativeArray<int> neighborData;

            public NativeArray<NodeVisitationStatus> NodeStatuses;

            public NativeArray<ClassificationJobStatus> status;
            public NativeArray<int> finalRegionIndex;

            private int currentRegionIndex;

            public void Execute()
            {
                DoExecute();
                DisposeAllAndComplete();
            }

            private void DisposeAllAndComplete()
            {
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

                    AssignAllPossibleToRegion(currentIndex, ((ulong)1) >> currentRegionIndex);
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

                var foundAtLeastOnePending = false;
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
                    infiniteProtector--;
                    if (infiniteProtector <= 0)
                    {
                        status[0] = ClassificationJobStatus.ERROR;
                        return;
                        //throw new Exception("Oh no, Infinite loop probably 2");
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
                        var neighborNodeIndex = neighborData[indexInNeighbors];
                        var neighborNodeStatus = NodeStatuses[neighborNodeIndex];
                        if (neighborNodeStatus == NodeVisitationStatus.COMPLETE)
                        {
                            continue;
                        }
                        var neighborNode = graphNodes[neighborNodeIndex];

                        if ((neighborNode.RegionMask & regionBitMask) != 0)
                        {
                            // this node has already been masked by the same bit mask as this node.
                            //  don't waste time doing anything else
                            continue;
                        }

                        neighborNode.RegionMask |= regionBitMask;
                        graphNodes[neighborNodeIndex] = neighborNode;

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
                        //throw new Exception("Oh no, Infinite loop probably");
                    }
                }
                return true;
            }
        }
    }
}
