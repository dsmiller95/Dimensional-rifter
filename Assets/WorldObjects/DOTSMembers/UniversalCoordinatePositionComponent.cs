using Assets.Tiling;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct UniversalCoordinatePositionComponent : IComponentData
    {
        public UniversalCoordinate coordinate;
    }
    public struct OffsetLayerComponent : IComponentData
    {
        /// <summary>
        /// ratio of how much z offset is applied per unit of y
        /// </summary>
        public float zOffsetPlaneSlope;
        public float netZOffset;
        public float3 ApplyPositionInOrderingLayer(float2 worldPosition)
        {
            return new float3(worldPosition, worldPosition.y * zOffsetPlaneSlope + netZOffset);
        }
    }
}
