using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    /// <summary>
    /// Represents a coordinate system of triangles of side length 1
    /// </summary>
    [CreateAssetMenu(fileName = "TriangleTilingSystem", menuName = "TileMap/TriangleTilingSystem", order = 1)]
    public class TriangleTileMapSystem : TileMapTileShapeStrategy<TriangleCoordinate>
    {
        public override ICoordinateSystem<TriangleCoordinate> GetBasicCoordinateSystem()
        {
            return new TriangleCoordinateSystem();
        }

        private static Vector3 BoundBoxSize = Vector3.one * 2f / Mathf.Sqrt(3);

        public override Bounds GetRawBounds(TriangleCoordinate coord, float sideLength, ICoordinateSystem<TriangleCoordinate> translateCoordinateSystem = null)
        {
            var position = translateCoordinateSystem.ToRealPosition(coord);
            return new Bounds(position, BoundBoxSize * sideLength);
        }

        public override int[] GetTileTriangles()
        {
            return new int[]
            {
                0, 1, 2
            };
        }

        public override IEnumerable<Vector2> GetVertexesAround(TriangleCoordinate coord, float sideLength, ICoordinateSystem<TriangleCoordinate> translateCoordinateSystem)
        {
            return TriangleCoordinateSystem.GetTriangleVertextesAround(coord, sideLength, translateCoordinateSystem);
        }
    }
}
