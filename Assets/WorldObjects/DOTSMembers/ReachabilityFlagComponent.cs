using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers
{
    public struct ReachabilityFlagComponent : IComponentData
    {
        public ulong RegionBitMask;
    }
}
