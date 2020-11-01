using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS
{
    public struct StorageDataComponent : IComponentData
    {
        public float TotalAdditionClaims;
        public float MaxCapacity;
    }
}
