using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.Tiling.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TriangleRhomboidRange", menuName = "TileMap/Ranges/TriangleRhomboidRange", order = 3)]
    public class TriangleRhomboidCoordinateRangeObject : CoordinateRangeObject<TriangleCoordinate>
    {
        public TriangleRhomboidCoordinateRange TriangleRange;
        public override ICoordinateRange<TriangleCoordinate> CoordinateRange => TriangleRange;
    }
}
