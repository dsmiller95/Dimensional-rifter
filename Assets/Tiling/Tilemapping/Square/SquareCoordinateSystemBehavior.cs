using Assets.Tiling.SquareCoords;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Square
{
    [ExecuteInEditMode]
    public class SquareCoordinateSystemBehavior : CoordinateSystemBehavior<SquareCoordinate>
    {
        protected override ICoordinateSystem<SquareCoordinate> BaseCoordinateSystem()
        {
            return new SquareCoordinateSystem();
        }
    }
}
