using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct TilemapSpawnerComponent : IComponentData
    {
        public SquareCoordinateRange spawningRange;
        public short planeIndex;
        public float timePerSpawn;
        public float nextSpawnTime;
    }
}
