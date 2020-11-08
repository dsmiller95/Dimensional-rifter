using Unity.Entities;

namespace Assets.WorldObjects.Members.Wall.DOTS
{
    public struct TileBlockingComponent : IComponentData
    {
        public bool CurrentlyBlocking;
    }
}
