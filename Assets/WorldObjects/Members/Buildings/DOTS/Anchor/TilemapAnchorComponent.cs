using Assets.Tiling;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Buildings.DOTS.Anchor
{
    [GenerateAuthoringComponent]
    public struct TilemapAnchorComponent : IComponentData
    {
        public UniversalCoordinate destinationCoordinate;
    }
}
