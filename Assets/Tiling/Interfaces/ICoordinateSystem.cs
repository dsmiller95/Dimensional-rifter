using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling
{
    public enum CoordinateSystemType
    {
        HEX,
        SQUARE,
        TRIANGLE
    }

    public interface ICoordinateSystem
    {
        CoordinateSystemType CoordType { get; }
    }

    /// <summary>
    /// Responsible only for transforming a type of coordinate to a specific point in space
    /// In a tileMap, the coordinates will represent the center of the tiles
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICoordinateSystem<T>: ICoordinateSystem where T : ICoordinate
    {
        IEnumerable<T> Neighbors(T coordinate);
        Vector2 ToRealPosition(T coordinate);
        T FromRealPosition(Vector2 realWorldPos);

        T DefaultCoordinate();

        /// <summary>
        /// compute a heuristic distance between two coordinates for use in A* pathfinding
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        float HeuristicDistance(T origin, T destination);
    }

}
