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
        public TriangleTileShape tileShape;
        public override IEnumerable<TileConfig<TriangleCoordinate>> GetTileConfigs()
        {
            return tileTypes
                .SelectMany(x => tileShape
                    .GenerateTileConfigsFromBaseSetup(x.ID, x.coords0)
                    );
        }
    }
}
