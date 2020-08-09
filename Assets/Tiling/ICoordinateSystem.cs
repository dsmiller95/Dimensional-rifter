using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling
{
    /// <summary>
    /// Responsible for representing the topology and shape of tiles in this system
    ///     will depend on a <see cref="ICoordinateSystem{T}"/> to modify the positioning of the
    ///     topology information that is returned
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITileMapTileShapeStrategy<T> where T : ICoordinate
    {
        Bounds GetRawBounds(T coord, float sideLength, ICoordinateSystem<T> translateCoordinateSystem);

        IEnumerable<Vector2> GetVertexesAround(T coord, float sideLength, ICoordinateSystem<T> translateCoordinateSystem = null);

        int[] GetTileTriangles();

        ICoordinateSystem<T> GetBasicCoordinateSystem();
    }

    /// <summary>
    /// Responsible only for transforming a type of coordinate to a specific point in space
    /// In a tileMap, the coordinates will represent the center of the tiles
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICoordinateSystem<T> where T : ICoordinate
    {
        IEnumerable<T> Neighbors(T coordinate);
        Vector2 ToRealPosition(T coordinate);
        T FromRealPosition(Vector2 realWorldPos);

        T DefaultCoordinate();
    }

    /// <summary>
    /// Interface used only to restrict the type constraints on other interfaces which deal with coordinates
    /// Currently empty
    /// </summary>
    public interface ICoordinate { }

    /// <summary>
    /// Responsible only for representing a range of <see cref="ICoordinate"/>s
    ///     Is also responsible for returning information about the topology of all of the tiles within this range
    ///     This is used when detecting if it overlaps with tiles in a TileMap which may not use the same coordinate system or tile shape
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICoordinateRange<T> : IEnumerable<T> where T : ICoordinate
    {
        /// <summary>
        /// Generate a collider for this 
        /// </summary>
        /// <returns></returns>
        IEnumerable<Vector2> BoundingPolygon(ICoordinateSystem<T> coordinateSystem, float individualScale);
    }
}
