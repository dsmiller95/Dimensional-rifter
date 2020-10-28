using Assets.Tiling;
using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct MemberPrefabComponent : IComponentData
    {
        public Entity prefab;
    }
}
