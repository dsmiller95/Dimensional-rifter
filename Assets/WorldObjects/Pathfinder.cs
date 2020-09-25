using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.Tilemapping.NEwSHITE;
using Assets.Tiling.TriangleCoords;
using Priority_Queue;
using Simulation.Tiling.HexCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects
{
    public class Pathfinder //<T> where T : struct, IBaseCoordinateType
    {
        public ISet<UniversalCoordinate> completed;

        class CoordinateData
        {
            public UniversalCoordinate coordinate;
            public UniversalCoordinate previous;
            public float distanceFromOrigin;
            public float heuristicValue;

            public float Priority => distanceFromOrigin + heuristicValue;

            public CoordinateData(
                UniversalCoordinate coord,
                UniversalCoordinate previous,
                UniversalCoordinate target,
                float currentDistanceFromOrigin)
            {
                coordinate = coord;
                this.previous = previous;
                distanceFromOrigin = currentDistanceFromOrigin;
                heuristicValue = coordinate.HeuristicDistance(target);
            }
        }

        private IDictionary<UniversalCoordinate, CoordinateData> visited;

        private SimplePriorityQueue<UniversalCoordinate, float> fringe;
        private TheReelBigCombinationTileMapManager bigboi;
        private Func<UniversalCoordinate, TileProperties, bool> coordinateFilterFunction;

        private readonly UniversalCoordinate origin;
        public Pathfinder(
            UniversalCoordinate origin,
            TheReelBigCombinationTileMapManager region,
            Func<UniversalCoordinate, TileProperties, bool> isCoordinateVisitable)
        {
            bigboi = region;
            coordinateFilterFunction = isCoordinateVisitable;
            this.origin = origin;

            fringe = new SimplePriorityQueue<UniversalCoordinate, float>();
            visited = new Dictionary<UniversalCoordinate, CoordinateData>();
            completed = new HashSet<UniversalCoordinate>();
        }

        public IEnumerable<UniversalCoordinate> RandomWalkOfLength(int length)
        {
            return InfiniteRandomWalkGenerator().Take(length);
        }

        private IEnumerable<UniversalCoordinate> InfiniteRandomWalkGenerator()
        {
            var currentPathTip = origin;
            while (true)
            {
                var neighbors = currentPathTip
                    .Neighbors()
                    .Where(neighbor => bigboi.ValidCoordinateInOwnPlane(neighbor)
                        && coordinateFilterFunction(neighbor, bigboi.everyMember.TilePropertiesAt(neighbor))
                    ).ToList();
                if(neighbors.Count <= 0)
                {
                    yield break;
                }
                currentPathTip = neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
                yield return currentPathTip;
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>the shortest path to <paramref name="destination"/> in reverse order, beginning with <paramref name="destination"/> and ending with the origin</returns>
        public IEnumerable<UniversalCoordinate> ShortestPathTo(UniversalCoordinate destination, bool overlapWithTarget = false)
        {
            try
            {
                return ShortestPathToGenerator(destination, overlapWithTarget);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private IEnumerable<UniversalCoordinate> ShortestPathToGenerator(UniversalCoordinate destination, bool overlapWithTarget)
        {
            var currentCoordinate = origin;
            var currentCoordinateData = new CoordinateData(currentCoordinate, default, destination, 0);
            visited[currentCoordinate] = currentCoordinateData;
            fringe.EnqueueWithoutDuplicates(currentCoordinateData.coordinate, currentCoordinateData.Priority);

            while (fringe.TryDequeue(out currentCoordinate) && !currentCoordinate.Equals(destination))
            {
                VisitNode(visited[currentCoordinate], destination);
                if (currentCoordinate.Equals(destination))
                {
                    Debug.Log("haheh");
                }
            }

            if (!currentCoordinate.Equals(destination))
            {
                return null;
            }
            else
            {
                return ExtractPathFromVisited(currentCoordinate, overlapWithTarget);
            }
        }

        private IEnumerable<UniversalCoordinate> ExtractPathFromVisited(UniversalCoordinate currentCoordinate, bool overlapWithTarget)
        {
            CoordinateData data = visited[currentCoordinate];
            while (!data.coordinate.Equals(origin))
            {
                yield return data.coordinate;
                data = visited[data.previous];
            }
            if (overlapWithTarget)
            {
                yield return data.coordinate;
            }
        }

        private void VisitNode(CoordinateData node, UniversalCoordinate destination)
        {
            var currentCoordinate = node.coordinate;
            completed.Add(currentCoordinate);

            var nonClosedNeighbors = currentCoordinate
                .Neighbors()
                .Where(neighbor => !completed.Contains(neighbor)
                    && bigboi.ValidCoordinateInOwnPlane(neighbor)
                    && coordinateFilterFunction(neighbor, bigboi.everyMember.TilePropertiesAt(neighbor))
                );

            // assumption is made that every connection is cost of 1
            var neighborDistance = node.distanceFromOrigin + 1;

            foreach (var neighbor in nonClosedNeighbors)
            {
                CoordinateData nodeData;
                if (!visited.TryGetValue(neighbor, out nodeData))
                {
                    nodeData = new CoordinateData(neighbor, currentCoordinate, destination, neighborDistance);
                    visited[neighbor] = nodeData;
                    fringe.EnqueueWithoutDuplicates(nodeData.coordinate, nodeData.Priority);
                }
                else
                {
                    if (nodeData.distanceFromOrigin > neighborDistance)
                    {
                        nodeData.previous = currentCoordinate;
                        nodeData.distanceFromOrigin = neighborDistance;
                    }
                }
            }
        }

        public static IEnumerable<UniversalCoordinate> PathBetween(
            UniversalCoordinate source,
            UniversalCoordinate destination,
            TheReelBigCombinationTileMapManager region,
            Func<UniversalCoordinate, TileProperties, bool> passableTiles,
            bool navigateToAdjacent)
        {

            var pather = new Pathfinder(destination, region, (coord, properties) => passableTiles(coord, properties));
            return pather.ShortestPathTo(source, !navigateToAdjacent);
        }
        public static IEnumerable<UniversalCoordinate> RandomWalk(
            UniversalCoordinate source,
            int steps,
            TheReelBigCombinationTileMapManager region,
            Func<UniversalCoordinate, TileProperties, bool> passableTiles)
        {
            var pather = new Pathfinder(source, region, (coord, properties) => passableTiles(coord, properties));
            return pather.RandomWalkOfLength(steps);
        }

    }

    public static class PathfinderUtils
    {
        public static IEnumerable<UniversalCoordinate> PathBetween(
            UniversalCoordinate source,
            UniversalCoordinate destination,
            TheReelBigCombinationTileMapManager region,
            Func<UniversalCoordinate, TileProperties, bool> passableTiles,
            bool navigateToAdjacent = true)
        {
            if(source.CoordinateMembershipData != destination.CoordinateMembershipData)
            {
                throw new ArgumentException("coordinates not compatable");
            }
            return Pathfinder.PathBetween(source, destination, region, passableTiles, navigateToAdjacent);
        }
        public static IEnumerable<UniversalCoordinate> RandomWalkOfLength(
            UniversalCoordinate source,
            int walkLength,
            TheReelBigCombinationTileMapManager region,
            Func<UniversalCoordinate, TileProperties, bool> passableTiles)
        {
            return Pathfinder.RandomWalk(source, walkLength, region, passableTiles);
        }
    }
}
