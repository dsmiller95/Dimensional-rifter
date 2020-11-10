using Assets.Tiling;
using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct UniversalCoordinatePositionComponent : IComponentData
    {
        public UniversalCoordinate coordinate;
    }
}
