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
    public abstract class TileMapTileShapeStrategy<T> : ScriptableObject where T : ICoordinate
    {
        public abstract Bounds GetRawBounds(T coord, float sideLength, ICoordinateSystem<T> translateCoordinateSystem);

        public abstract IEnumerable<Vector2> GetVertexesAround(T coord, float sideLength, ICoordinateSystem<T> translateCoordinateSystem = null);

        public abstract int[] GetTileTriangles();

        public abstract ICoordinateSystem<T> GetBasicCoordinateSystem();
    }
}
