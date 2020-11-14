using Unity.Entities;

namespace Assets.WorldObjects.Members.Wall.DOTS
{
    [GenerateAuthoringComponent]
    public struct TileBlockingComponent : IComponentData
    {
        public bool CurrentlyBlocking;
    }
}
