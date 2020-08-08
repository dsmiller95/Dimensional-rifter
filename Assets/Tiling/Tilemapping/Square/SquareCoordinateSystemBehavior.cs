using Assets.Tiling.SquareCoords;

namespace Assets.Tiling.Tilemapping.Square
{
    public class SquareCoordinateSystemBehavior : CoordinateSystemBehavior<SquareCoordinate>
    {
        protected override ICoordinateSystem<SquareCoordinate> BaseCoordinateSystem()
        {
            return new SquareCoordinateSystem();
        }
    }
}
