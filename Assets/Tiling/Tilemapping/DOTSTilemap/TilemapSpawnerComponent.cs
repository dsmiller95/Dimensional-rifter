using Assets.Tiling.SquareCoords;
using Unity.Entities;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct TilemapSpawnerComponent : IComponentData
    {
        public RectCoordinateRange spawningRange;
        public Entity spawnedParent;
        public float timePerSpawn;
        public float nextSpawnTime;
    }
}
