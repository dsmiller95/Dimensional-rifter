using Assets.Tiling.TriangleCoords;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Triangle
{
    [ExecuteInEditMode]
    public class TriangleCoordinateSystemBehavior : CoordinateSystemBehavior<TriangleCoordinate>
    {
        public TriangleTileMapTile[] tileTypes;

        protected override ICoordinateSystem<TriangleCoordinate> BaseCoordinateSystem()
        {
            return new TriangleCoordinateSystem();
        }
    }
}
