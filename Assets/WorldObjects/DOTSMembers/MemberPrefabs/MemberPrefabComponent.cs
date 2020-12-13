using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers.MemberPrefabs
{
    [GenerateAuthoringComponent]
    public struct MemberPrefabComponent : IComponentData
    {
        public Entity prefab;
    }
}
