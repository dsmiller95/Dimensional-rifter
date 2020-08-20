using Assets.Tiling.SquareCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.TileConfiguration
{
    [Serializable]
    public struct SquareTileMapTile
    {
        public SquareCoordinate coords0;
        public string baseID;
    }

    [CreateAssetMenu(fileName = "SquareTileSet", menuName = "TileMap/TileSets/Square", order = 1)]
    public class SquareTileSet : TileSet<SquareCoordinate>
    {
        public SquareTileMapTile[] tileTypes;
        public SquareTileShape tileShape;
        public override IEnumerable<TileConfig<SquareCoordinate>> GetTileConfigs()
        {
            return tileTypes
                .SelectMany(x => tileShape
                    .GenerateTileConfigsFromBaseSetup(x.baseID, x.coords0)
                    );
        }
    }
}
