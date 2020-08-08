using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    /// <summary>
    /// Represents a coordinate system of squares of side length 1
    /// </summary>
    public class SquareTileMapSystem : ITileMapSystem<SquareCoordinate>
    {
        public ICoordinateSystem<SquareCoordinate> GetBasicCoordinateSystem()
        {
            return new SquareCoordinateSystem();
        }

        public int[] GetTileTriangles()
        {
            return new int[]
            {
                0, 1, 2,
                2, 3, 0
            };
        }

        public IEnumerable<Vector2> GetVertexesAround(SquareCoordinate coord, float sideLength, ICoordinateSystem<SquareCoordinate> translateCoordinateSystem)
        {
            return SquareCoordinateSystem.GetSquareVertsAround(coord, sideLength, translateCoordinateSystem);
        }
    }
}
