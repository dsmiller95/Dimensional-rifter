using Unity.Entities;
using Unity.Mathematics;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct OffsetFromCoordinatePositionComponent : IComponentData
    {
        public float2 Value;
    }
}
