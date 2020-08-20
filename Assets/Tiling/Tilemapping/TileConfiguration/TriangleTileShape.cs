using Assets.Tiling.SquareCoords;
using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
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
    public struct TriangleTileShapeOffset
    {
        public TriangleCoordinate coords0;
        public TriangleTileShapes shape;
    }

    [CreateAssetMenu(fileName = "TriangleTileShape", menuName = "TileMap/TileSets/TriangleShape", order = 2)]
    public class TriangleTileShape : ScriptableObject
    {
        public TriangleTileShapeOffset[] tileShapes;
        public IEnumerable<TileConfig<TriangleCoordinate>> GenerateTileConfigsFromBaseSetup(string baseName, TriangleCoordinate coordinate)
        {
            foreach (var shape in tileShapes)
            {
                yield return new TileConfig<TriangleCoordinate>()
                {
                    tileCoordinate = coordinate + shape.coords0,
                    typeIdentifier = new TileTypeInfo(baseName, GetPrefix(shape.shape))
                };
            }
        }

        public static string GetPrefix(TriangleTileShapes shapeType)
        {
            return Enum.GetName(typeof(TriangleTileShapes), shapeType);
        }
    }
}
