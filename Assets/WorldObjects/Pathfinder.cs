using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.TriangleCoords;
using Microsoft.Win32.SafeHandles;
using Priority_Queue;
using Simulation.Tiling.HexCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.WorldObjects
{
    public class Pathfinder<T> where T : ICoordinate
    {
        public ISet<T> completed;

        class CoordinateData
        {
            public T coordinate;
            public T previous;
            public float distanceFromOrigin;
            public float heuristicValue;
            private ICoordinateSystem<T> coordinateSystem;

            public float Priority => distanceFromOrigin + heuristicValue;

            public CoordinateData(
                T coord,
                T previous,
                T target,
                float currentDistanceFromOrigin, ICoordinateSystem<T> coordinateSystem)
            {
                this.coordinateSystem = coordinateSystem;
                coordinate = coord;
                this.previous = previous;
                distanceFromOrigin = currentDistanceFromOrigin;
                heuristicValue = this.coordinateSystem.HeuristicDistance(coordinate, target);
            }
        }

        private IDictionary<T, CoordinateData> visited;

        private SimplePriorityQueue<T, float> fringe;
        private ICoordinateSystem<T> coordinateSystem;
        private TileMapRegion<T> tileRegion;
        private Func<T, TileProperties, bool> coordinateFilterFunction;

        private readonly T origin;
        public Pathfinder(T origin, TileMapRegion<T> region, Func<T, TileProperties, bool> isCoordinateVisitable)
        {
            tileRegion = region;
            this.coordinateSystem = region.WorldSpaceCoordinateSystem;
            coordinateFilterFunction = isCoordinateVisitable;
            this.origin = origin;

            fringe = new SimplePriorityQueue<T, float>();
            visited = new Dictionary<T, CoordinateData>();
            completed = new HashSet<T>();
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="destination"></param>
        /// <returns>the shortest path to <paramref name="destination"/> in reverse order, beginning with <paramref name="destination"/> and ending with the origin</returns>
        public IEnumerable<T> ShortestPathTo(T destination)
        {
            try
            {
                return ShortestPathToGenerator(destination);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        private IEnumerable<T> ShortestPathToGenerator(T destination)
        {
            var currentCoordinate = origin;
            var currentCoordinateData = new CoordinateData(currentCoordinate, default, destination, 0, coordinateSystem);
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
            }else
            {
                return ExtractPathFromVisited(currentCoordinate);
            }
        }

        private IEnumerable<T> ExtractPathFromVisited(T currentCoordinate)
        {
            CoordinateData data = visited[currentCoordinate];
            while (!data.coordinate.Equals(origin))
            {
                yield return data.coordinate;
                data = visited[data.previous];
            }
        }

        private void VisitNode(CoordinateData node, T destination)
        {
            var currentCoordinate = node.coordinate;
            completed.Add(currentCoordinate);

            var nonClosedNeighbors = coordinateSystem
                .Neighbors(currentCoordinate)
                .Where(neighbor => !completed.Contains(neighbor)
                    && tileRegion.IsValidCoordinate(neighbor)
                    && coordinateFilterFunction(neighbor, tileRegion.contentTracker.TilePropertiesAt(neighbor))
                );

            // assumption is made that every connection is cost of 1
            var neighborDistance = node.distanceFromOrigin + 1;

            foreach (var neighbor in nonClosedNeighbors)
            {
                CoordinateData nodeData;
                if (!visited.TryGetValue(neighbor, out nodeData))
                {
                    nodeData = new CoordinateData(neighbor, currentCoordinate, destination, neighborDistance, coordinateSystem);
                    visited[neighbor] = nodeData;
                    fringe.EnqueueWithoutDuplicates(nodeData.coordinate, nodeData.Priority);
                }
                else
                {
                    if(nodeData.distanceFromOrigin > neighborDistance)
                    {
                        nodeData.previous = currentCoordinate;
                        nodeData.distanceFromOrigin = neighborDistance;
                    }
                }
            }
        }


        public static IEnumerable<T> PathBetween(ICoordinate source, ICoordinate destination, TileMapRegionNoCoordinateType coordinateSystem, Func<ICoordinate, TileProperties, bool> passableTiles)
        {
            var coordSystem = (coordinateSystem as TileMapRegion<T>);

            var pather = new Pathfinder<T>((T)destination, coordSystem, (coord, properties) => passableTiles(coord, properties));
            return pather.ShortestPathTo((T)source);
        }

    }

    public static class PathfinderUtils
    {
        public static IEnumerable<ICoordinate> PathBetween(
            ICoordinate source,
            ICoordinate destination,
            TileMapRegionNoCoordinateType region,
            Func<ICoordinate, TileProperties, bool> passableTiles)
        {
            var coordinateSpace = region.UntypedCoordianteSystemWorldSpace;

            if (!coordinateSpace.IsCompatible(source) || !coordinateSpace.IsCompatible(destination))
            {
                throw new ArgumentException("coordinates not compatable");
            }

            switch (coordinateSpace.CoordType)
            {
                case CoordinateSystemType.HEX:
                    return Pathfinder<AxialCoordinate>.PathBetween(source, destination, region, passableTiles)?
                        .Cast<ICoordinate>();
                case CoordinateSystemType.SQUARE:
                    return Pathfinder<SquareCoordinate>.PathBetween(source, destination, region, passableTiles)?
                        .Cast<ICoordinate>();
                case CoordinateSystemType.TRIANGLE:
                    return Pathfinder<TriangleCoordinate>.PathBetween(source, destination, region, passableTiles)?
                        .Cast<ICoordinate>();
            }
            throw new ArgumentException("invalid coordinate system type");
        }
    }
}
