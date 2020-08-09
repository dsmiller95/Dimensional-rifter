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

        public override int GetHashCode()
        {
            return (row << 16) ^ column;
        }

        public override bool Equals(object obj)
        {
            if (obj is SquareCoordinate coord)
            {
                return coord.row == row && coord.column == column;
            }
            return false;
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


        private static readonly Vector2[] squareVerts = new Vector2[] {
                new Vector3(-.5f,-.5f),
                new Vector3(-.5f, .5f),
                new Vector3( .5f, .5f),
                new Vector3( .5f,-.5f),};
        /// <summary>
        /// Get a list of vertexes representing the square around the given square coordinate, with a side length of <paramref name="squareScale"/>
        /// </summary>
        /// <param name="coord">The sqaure coordinate</param>
        /// <param name="coordinateSystem">The coordinate system to use to translate the position of the verts</param>
        /// <param name="squareScale">the scale</param>
        /// <returns>an IEnumerable of 4 vertextes representing the square, rotating clockwise around the center</returns>
        public static IEnumerable<Vector2> GetSquareVertsAround(SquareCoordinate coord, float squareScale, ICoordinateSystem<SquareCoordinate> coordinateSystem = null)
        {
            IEnumerable<Vector2> verts = squareVerts;
            verts = verts.Select(x => x * squareScale);
            if (coordinateSystem != null)
            {
                var location = coordinateSystem.ToRealPosition(coord);
                verts = verts.Select(x => x + location);
            }
            return verts;
        }

        public SquareCoordinate DefaultCoordinate()
        {
            return new SquareCoordinate(0, 0);
        }
    }
}
