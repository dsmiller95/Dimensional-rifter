using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.DOTS
{
    public class RemoveTransformData : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.RemoveComponent<Translation>(entity);
            dstManager.RemoveComponent<Rotation>(entity);
            dstManager.RemoveComponent<LocalToWorld>(entity);
        }
    }
}
