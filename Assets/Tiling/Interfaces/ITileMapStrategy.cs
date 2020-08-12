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
}
