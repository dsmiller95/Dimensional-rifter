using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.DOTSMembers
{
    public class OffsetLayerComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public TileMemberOrderingLayer orderingLayer;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var orderingComponent = orderingLayer.ToECSComponent();
            dstManager.AddComponentData(entity, orderingComponent);
        }
    }
}
