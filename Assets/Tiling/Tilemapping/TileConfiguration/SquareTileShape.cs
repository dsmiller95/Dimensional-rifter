using Assets.Tiling.SquareCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.TileConfiguration
{
    public enum SqaureTileShape
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
        public SqaureTileShape shape;
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
                    ID = baseName + GetPrefix(shape.shape)
                };
            }
        }

        public static string GetPrefix(SqaureTileShape shapeType) {
            return Enum.GetName(typeof(SqaureTileShape), shapeType);
        }
    }
}
