﻿using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Microsoft.Win32.SafeHandles;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private TileMapRegion<T> region;

        private readonly T origin;
        public Pathfinder(T origin, TileMapRegion<T> region)
        {
            this.region = region;
            this.coordinateSystem = region.WorldSpaceCoordinateSystem;
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
                .Where(neighbor => region.IsValidCoordinate(neighbor) && !completed.Contains(neighbor));

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
                    nodeData.distanceFromOrigin = Math.Min(nodeData.distanceFromOrigin, neighborDistance);
                }
            }
        }
    }
}
