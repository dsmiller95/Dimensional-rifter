using Assets.Tiling.SquareCoords;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Square
{
    [ExecuteInEditMode]
    public class SquareCoordinateSystemBehavior : CoordinateSystemTransformBehavior<SquareCoordinate>
    {
        public override ICoordinateSystem<SquareCoordinate> BaseCoordinateSystem =>
            new SquareCoordinateSystem();
    }
}
