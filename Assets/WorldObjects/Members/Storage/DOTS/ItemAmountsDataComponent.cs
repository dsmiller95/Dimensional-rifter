using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS
{
    public struct ItemAmountsDataComponent : IComponentData
    {
        public float TotalAdditionClaims;
        public float MaxCapacity;
        public bool LockItemDataBufferTypes;
    }
}
