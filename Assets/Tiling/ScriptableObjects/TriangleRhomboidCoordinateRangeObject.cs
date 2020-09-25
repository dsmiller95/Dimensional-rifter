using Assets.Tiling.Tilemapping.NEwSHITE;
using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.Tiling.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TriangleRhomboidRange", menuName = "TileMap/Ranges/TriangleRhomboidRange", order = 3)]
    public class TriangleRhomboidCoordinateRangeObject : CoordinateRangeObject
    {
        public TriangleRhomboidCoordinateRange RhomboidRange;
        public override IUniversalCoordinateRange CoordinateRange => new TriangleRangeUniversalContainer(RhomboidRange);
    }
}
