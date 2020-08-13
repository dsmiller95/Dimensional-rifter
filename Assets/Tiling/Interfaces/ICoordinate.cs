using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling
{
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

        bool ContainsCoordinate(T coordinat);
    }
}
