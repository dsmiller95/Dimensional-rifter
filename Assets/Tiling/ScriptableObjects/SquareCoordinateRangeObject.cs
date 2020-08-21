using Assets.Tiling.SquareCoords;
using UnityEngine;

namespace Assets.Tiling.ScriptableObjects
{
    [CreateAssetMenu(fileName = "SquareCoordinateRange", menuName = "TileMap/Ranges/SqaureRange", order = 1)]
    public class SquareCoordinateRangeObject : CoordinateRangeObject<SquareCoordinate>
    {
        public SquareCoordinateRange SquareRange;
        public override ICoordinateRange<SquareCoordinate> CoordinateRange => SquareRange;
    }
}
