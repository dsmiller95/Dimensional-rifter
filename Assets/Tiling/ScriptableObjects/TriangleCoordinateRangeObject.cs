using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.Tiling.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TriangleCoordinateRange", menuName = "TileMap/Ranges/TriangleRange", order = 2)]
    public class TriangleCoordinateRangeObject : CoordinateRangeObject<TriangleCoordinate>
    {
        public TriangleTriangleCoordinateRange TriangleRange;
        public override ICoordinateRange<TriangleCoordinate> CoordinateRange => TriangleRange;
    }
}
