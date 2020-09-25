using Assets.Tiling.Tilemapping.NEwSHITE;
using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.Tiling.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TriangleCoordinateRange", menuName = "TileMap/Ranges/TriangleRange", order = 2)]
    public class TriangleCoordinateRangeObject : CoordinateRangeObject
    {
        public TriangleTriangleCoordinateRange TriangleRange;
        public override IUniversalCoordinateRange CoordinateRange => new TriangleRangeUniversalContainer(TriangleRange);
    }
}
