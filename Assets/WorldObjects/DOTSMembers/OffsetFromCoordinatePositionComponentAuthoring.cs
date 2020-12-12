using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.WorldObjects.DOTSMembers
{
    public struct OffsetFromCoordinatePositionComponent : IComponentData
    {
        public float2 Value;
    }

    public class OffsetFromCoordinatePositionComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public float2 Offset;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData<OffsetFromCoordinatePositionComponent>(entity, new OffsetFromCoordinatePositionComponent
            {
                Value = Offset
            });
        }
    }
}
