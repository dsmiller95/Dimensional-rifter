using Assets.WorldObjects.Members;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.DOTSMembers.MemberPrefabs
{
    public struct MemberPrefabIDComponent : IComponentData
    {
        public int prefabID;
    }

    public class MemberPrefabIDComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public MemberType memberType;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new MemberPrefabIDComponent
            {
                prefabID = memberType.memberID
            });
        }
    }
}
