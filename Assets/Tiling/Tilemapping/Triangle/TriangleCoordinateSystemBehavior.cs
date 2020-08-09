using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Triangle
{
    [ExecuteInEditMode]
    public class TriangleCoordinateSystemBehavior : CoordinateSystemTransformBehavior<TriangleCoordinate>
    {
        public override ICoordinateSystem<TriangleCoordinate> BaseCoordinateSystem =>
            new TriangleCoordinateSystem();
    }
}
