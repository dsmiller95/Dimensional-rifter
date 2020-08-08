using Assets.Tiling.TriangleCoords;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Tiling.Tilemapping.Triangle
{
    [System.Serializable]
    public struct TriangleTileMapTile
    {
        public TriangleCoordinate coords0;
        public string ID;
    }
    public class TriangleTileSet : TileSet<TriangleCoordinate>
    {
        public TriangleTileMapTile[] tileTypes;
        public override IEnumerable<TileConfig<TriangleCoordinate>> GetTileConfigs()
        {
            return tileTypes.Select(x => new TileConfig<TriangleCoordinate>
            {
                ID = x.ID,
                tileCoordinate = x.coords0
            });
        }
    }
}
