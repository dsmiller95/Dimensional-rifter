using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging
{
    public struct StorageSupplyErrandResultComponent : IComponentData
    {
        public Entity itemSource;
        public Entity supplyTarget;
        public Resource resourceTransferType;
        public float amountToTransfer;
    }
}
