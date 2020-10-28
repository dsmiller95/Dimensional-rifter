using Assets.Tiling;
using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct UniversalCoordinatePosition : IComponentData
    {
        public UniversalCoordinate coordinate;
    }
}
