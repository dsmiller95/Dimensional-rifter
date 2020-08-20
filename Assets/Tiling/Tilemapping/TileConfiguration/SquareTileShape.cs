using Assets.Tiling.SquareCoords;
using Assets.WorldObjects;
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

    [CreateAssetMenu(fileName = "SquareTileShape", menuName = "TileSets/SquareShape", order = 2)]
    public class SquareTileShape : ScriptableObject
    {
        public SquareTileShapeOffset[] tileShapes;
        public IEnumerable<TileConfig<SquareCoordinate>> GenerateTileConfigsFromBaseSetup(string baseName, SquareCoordinate coordinate)
        {
            foreach (var shape in tileShapes)
            {
                yield return new TileConfig<SquareCoordinate>()
                {
                    tileCoordinate = coordinate + shape.coords0,
                    typeIdentifier = new TileTypeInfo(baseName, GetPrefix(shape.shape))
                };
            }
        }

        public static string GetPrefix(SquareTileShapes shapeType)
        {
            return Enum.GetName(typeof(SquareTileShapes), shapeType);
        }
    }
}
