using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    /// <summary>
    /// Represents a coordinate system of squares of side length 1
    /// </summary>
    public class TriangleTileMapSystem : ITileMapSystem<TriangleCoordinate>
    {
        public ICoordinateSystem<TriangleCoordinate> GetBasicCoordinateSystem()
        {
            return new TriangleCoordinateSystem();
        }

        public int[] GetTileTriangles()
        {
            return new int[]
            {
                0, 1, 2
            };
        }

        public IEnumerable<Vector2> GetVertexesAround(TriangleCoordinate coord, float sideLength, ICoordinateSystem<TriangleCoordinate> translateCoordinateSystem)
        {
            return TriangleCoordinateSystem.GetTriangleVertextesAround(coord, sideLength, translateCoordinateSystem);
        }
    }
}
