using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    [GenerateAuthoringComponent]
    public struct LooseItemPrefabComponent : IComponentData
    {
        public Resource type;
        public Entity looseItemPrefab;
    }
}
