using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    public struct LooseItemSpawnCommandComponent : IComponentData
    {
        public Resource type;
        public float amount;
    }
}
