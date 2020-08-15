using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.TileConfiguration
{
    [System.Serializable]
    public struct TriangleTileMapTile
    {
        public TriangleCoordinate coords0;
        public string ID;
    }

    [CreateAssetMenu(fileName = "TriangleTileSet", menuName = "TileSets/Triangle", order = 3)]
    public class TriangleTileSet : TileSet<TriangleCoordinate>
    {
        public TriangleTileMapTile[] tileTypes;
        public override IEnumerable<TileConfig<TriangleCoordinate>> GetTileConfigs()
        {
            return tileTypes.Select(x => new TileConfig<TriangleCoordinate>
            {
                typeIdentifier = new TileTypeInfo(x.ID, ""),
                tileCoordinate = x.coords0
            });
        }
    }
}
