using Assets.Tiling.TriangleCoords;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.TileConfiguration
{
    public enum TriangleTileShapes
    {
        FULL_BORDERS,
        NO_BORDERS,

        TWOEDGES0,
        TWOEDGES1,
        TWOEDGES2,

        ONEEDGE0,
        ONEEDGE1,
        ONEEDGE2,
    }

    [Serializable]
    public struct TriangleTileMapTile
    {
        public TriangleCoordinateStructSystem coords0;
        public string baseID;
    }

    [Serializable]
    public struct TriangleTileShapeOffset
    {
        public TriangleCoordinateStructSystem coords0;
        public TriangleTileShapes shape;
    }

    [CreateAssetMenu(fileName = "TriangleTileSet", menuName = "TileMap/TileSets/Triangle", order = 1)]
    public class TriangleTileSet : TileSet
    {
        public TriangleTileMapTile[] tileTypes;
        public TriangleTileShapeOffset[] tileShapes;

        public override IEnumerable<TileCoordinates> GetTileConfigs()
        {
            foreach (var tileType in tileTypes)
            {
                foreach (var shape in tileShapes)
                {
                    yield return new TileCoordinates()
                    {
                        tileCoordinate = UniversalCoordinate.From(tileType.coords0 + shape.coords0, 0),
                        typeIdentifier = new TileTypeInfo(tileType.baseID, GetPrefix(shape.shape))
                    };
                }
            }
        }
        public static string GetPrefix(TriangleTileShapes shapeType)
        {
            return Enum.GetName(typeof(TriangleTileShapes), shapeType);
        }
    }
}
