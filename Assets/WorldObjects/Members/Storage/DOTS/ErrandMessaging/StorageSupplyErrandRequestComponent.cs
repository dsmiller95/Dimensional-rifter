using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging
{
    public struct StorageSupplyErrandRequestComponent : IComponentData
    {
        /// <summary>
        /// Used to ensure the request isn't read before the commandbuffer writes the data
        ///     into the component. Since these components are being written on the main thread
        ///     I don't want to assume anything about what systems are running while they are created
        /// </summary>
        public bool DataIsSet;
        public uint SupplyTargetType;
        public uint ItemSourceTypeFlags;
    }
}
