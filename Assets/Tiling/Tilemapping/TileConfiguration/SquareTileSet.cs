using Assets.Tiling.SquareCoords;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.TileConfiguration
{
    public enum SquareTileShapes
    {
        FULL_BORDERS,
        NO_BORDERS,

        EDGESX,
        EDGESY,

        EDGE_R,
        EDGE_B,
        EDGE_L,
        EDGE_T,

        CORNER_BR,
        CORNER_BL,
        CORNER_TR,
        CORNER_TL,

        ALLEXCEPT_R,
        ALLEXCEPT_B,
        ALLEXCEPT_L,
        ALLEXCEPT_T
    }

    [Serializable]
    public struct SquareTileShapeOffset
    {
        public SquareCoordinate coords0;
        public SquareTileShapes shape;
    }

    [Serializable]
    public struct SquareTileMapTile
    {
        public SquareCoordinate coords0;
        public string baseID;
    }

    [CreateAssetMenu(fileName = "SquareTileSet", menuName = "TileMap/TileSets/Square", order = 1)]
    public class SquareTileSet : TileSet
    {
        public SquareTileMapTile[] tileTypes;
        public SquareTileShapeOffset[] tileShapes;

        public override IEnumerable<TileCoordinates> GetTileConfigs()
        {
            foreach (var tileType in tileTypes)
            {
                foreach (var tileShape in tileShapes)
                {
                    yield return new TileCoordinates()
                    {
                        tileCoordinate = UniversalCoordinate.From(tileType.coords0 + tileShape.coords0, 0),
                        typeIdentifier = new TileTypeInfo(tileType.baseID, GetPrefix(tileShape.shape))
                    };
                }
            }
        }
        public static string GetPrefix(SquareTileShapes shapeType)
        {
            return Enum.GetName(typeof(SquareTileShapes), shapeType);
        }
    }
}
