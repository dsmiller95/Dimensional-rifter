using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct TilemapRegionRoot : IComponentData
    {
        public short planeIndex;
    }
}
