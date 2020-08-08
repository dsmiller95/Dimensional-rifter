using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    [Serializable]
    public struct SquareCoordinate : ICoordinate
    {
        public SquareCoordinate(int row, int col)
        {
            this.row = row;
            column = col;
        }
        public int row;
        public int column;

        public static SquareCoordinate operator +(SquareCoordinate a, SquareCoordinate b)
        {
            return new SquareCoordinate(a.row + b.row, a.column + b.column);
        }

        public static readonly SquareCoordinate UP = new SquareCoordinate(1, 0);
        public static readonly SquareCoordinate DOWN = new SquareCoordinate(-1, 0);
        public static readonly SquareCoordinate RIGHT = new SquareCoordinate(0, 1);
        public static readonly SquareCoordinate LEFT = new SquareCoordinate(0, -1);

        public override string ToString()
        {
            return $"row: {row}, col: {column}";
        }
    }


    /// <summary>
    /// Represents a coordinate system of squares of side length 1
    /// </summary>
    public class SquareCoordinateSystem : ICoordinateSystem<SquareCoordinate>
    {
        public SquareCoordinate FromRealPosition(Vector2 realWorldPos)
        {
            var row = Mathf.RoundToInt(realWorldPos.y);
            var col = Mathf.RoundToInt(realWorldPos.x);
            return new SquareCoordinate(row, col);
        }

        public IEnumerable<SquareCoordinate> Neighbors(SquareCoordinate coordinate)
        {
            return new[]
            {
                SquareCoordinate.UP,
                SquareCoordinate.DOWN,
                SquareCoordinate.LEFT,
                SquareCoordinate.RIGHT,
            }.Select(x => x + coordinate);
        }

        public Vector2 ToRealPosition(SquareCoordinate coordinate)
        {
            return new Vector2(coordinate.column, coordinate.row);
        }
    }
}
