using Assets.Tiling.SquareCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Square
{
    [Serializable]
    public struct SquareTileMapTile
    {
        public SquareCoordinate coords0;
        public string ID;
    }

    [CreateAssetMenu(fileName = "SquareTileSet", menuName = "TileSets/Square", order = 1)]
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
