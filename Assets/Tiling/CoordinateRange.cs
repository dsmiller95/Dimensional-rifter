using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling
{
    public interface IUniversalCoordinateRange
    {
        CoordinateType coordinateType { get; }

        /// <summary>
        /// bounds in-plane of the coordinates
        /// </summary>
        /// <returns></returns>
        IEnumerable<Vector2> BoundingPolygon();

        /// <summary>
        /// Get a list of coordinates which define the most extreme points in this coordinate range
        /// </summary>
        /// <returns></returns>

        bool ContainsCoordinate(UniversalCoordinate coordinate);

        IEnumerable<UniversalCoordinate> GetUniversalCoordinates(short coordPlaneID = 0);

        int TotalCoordinateContents();
    }

    /// <summary>
    /// Responsible only for representing a range of <see cref="ICoordinate"/>s
    ///     Is also responsible for returning information about the topology of all of the tiles within this range
    ///     This is used when detecting if it overlaps with tiles in a TileMap which may not use the same coordinate system or tile shape
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICoordinateRangeNEW<T> : IEnumerable<T> where T : struct, IBaseCoordinateType
    {
        /// <summary>
        /// bounds in-plane of the coordinates
        /// </summary>
        /// <returns></returns>
        IEnumerable<Vector2> BoundingPolygon();

        /// <summary>
        /// Get a list of coordinates which define the most extreme points in this coordinate range
        /// </summary>
        /// <returns></returns>
        bool ContainsCoordinate(T coordinate);

        int TotalCoordinateContents();
    }
}
