using Assets.Tiling.SquareCoords;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Tiling.Tilemapping.Square
{
    [Serializable]
    public struct SquareTileMapTile
    {
        public SquareCoordinate coords0;
        public string ID;
    }

    public class SquareTileSet : TileSet<SquareCoordinate>
    {
        public SquareTileMapTile[] tileTypes;
        public override IEnumerable<TileConfig<SquareCoordinate>> GetTileConfigs()
        {
            return tileTypes.Select(x => new TileConfig<SquareCoordinate>
            {
                ID = x.ID,
                tileCoordinate = x.coords0
            });
        }
    }
}
