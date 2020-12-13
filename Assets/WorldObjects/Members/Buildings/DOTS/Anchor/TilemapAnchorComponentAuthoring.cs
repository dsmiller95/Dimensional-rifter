using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.Anchor
{
    public struct TilemapAnchorComponent : IComponentData
    {
        public short AnchoredTileMap;
    }

    public class TilemapAnchorComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public short AnchoredTileMapIndex;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new TilemapAnchorComponent
            {
                AnchoredTileMap = AnchoredTileMapIndex
            });
        }
    }
}
