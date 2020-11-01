using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging
{
    public struct StorageSupplyErrandRequestComponent : IComponentData
    {
        public uint SupplyTargetType;
        public uint ItemSourceTypeFlags;
    }
}
