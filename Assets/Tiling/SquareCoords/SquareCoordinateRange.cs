using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)] // total size: 16 bytes
    public struct SquareCoordinateRange : ICoordinateRange<SquareCoordinate>, IEquatable<SquareCoordinateRange>
    {
        /// <summary>
        /// beginning of the range (inclusive)
        /// </summary>
        [FieldOffset(0)] public SquareCoordinate coord0;
        [FieldOffset(8)] public int rows;
        [FieldOffset(12)] public int cols;

        public SquareCoordinate MaximumBound => coord0 + new SquareCoordinate(rows - 1, cols - 1);


        public IEnumerator<SquareCoordinate> GetEnumerator()
        {
            for (var column = 0; column < cols; column++)
            {
                for (var row = 0; row < rows; row++)
                {
                    yield return new SquareCoordinate(coord0.row + row, coord0.column + column);
                }
            }
        }

        public SquareCoordinate AtIndex(int index)
        {
            var column = index / rows;
            var row = index % rows;
            return new SquareCoordinate(coord0.row + row, coord0.column + column);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<SquareCoordinate>).GetEnumerator();
        }

        public IEnumerable<Vector2> BoundingPolygon()
        {
            var halfScale = 1 / 2;

            var nextPos = coord0.ToPositionInPlane();
            yield return (Vector2)nextPos - Vector2.one * halfScale;

            var nextCoord = new SquareCoordinate(coord0.row + rows, coord0.column);
            nextPos = nextCoord.ToPositionInPlane();
            yield return (Vector2)nextPos + new Vector2(-1, 1) * halfScale;

            nextCoord = new SquareCoordinate(coord0.row + rows, coord0.column + cols);
            nextPos = nextCoord.ToPositionInPlane();
            yield return (Vector2)nextPos + Vector2.one * halfScale;

            nextCoord = new SquareCoordinate(coord0.row, coord0.column + cols);
            nextPos = nextCoord.ToPositionInPlane();
            yield return (Vector2)nextPos + new Vector2(1, -1) * halfScale;
        }

        public bool ContainsCoordinate(UniversalCoordinate universalCoordinate)
        {
            if (universalCoordinate.type != CoordinateType.SQUARE)
            {
                return false;
            }
            return ContainsCoordinate(universalCoordinate.squareDataView);
        }

        public bool ContainsCoordinate(SquareCoordinate coordinate)
        {
            var diff = coordinate - coord0;
            return (diff.column >= 0 && diff.column < cols) &&
                (diff.row >= 0 && diff.row < rows);
        }

        public SquareCoordinate GetRandomCoordinate(ref Unity.Mathematics.Random randomSource)
        {
            var row = randomSource.NextInt(0, rows);
            var col = randomSource.NextInt(0, cols);
            return new SquareCoordinate(row, col) + coord0;
        }

        public override string ToString()
        {
            return $"({coord0})-({rows} rows, {cols} columns)";
        }

        public int TotalCoordinateContents()
        {
            return rows * cols;
        }

        public bool Equals(SquareCoordinateRange other)
        {
            return coord0.Equals(other.coord0) && rows == other.rows && cols == other.cols;
        }
        public override bool Equals(object obj)
        {
            if (obj is SquareCoordinateRange typed)
            {
                return typed == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return rows << 16 + cols;
        }

        public static bool operator ==(SquareCoordinateRange a, SquareCoordinateRange b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(SquareCoordinateRange a, SquareCoordinateRange b)
        {
            return !a.Equals(b);
        }
        public static SquareCoordinateRange FromCoordsLargestExclusive(SquareCoordinate startCoord, SquareCoordinate endCoord)
        {
            EnsureCoordOrdering(ref startCoord, ref endCoord);
            var diff = endCoord - startCoord;
            return new SquareCoordinateRange()
            {
                coord0 = startCoord,
                rows = diff.row,
                cols = diff.column
            };
        }
        public static SquareCoordinateRange FromCoordsInclusive(SquareCoordinate startCoord, SquareCoordinate endCoord)
        {
            EnsureCoordOrdering(ref startCoord, ref endCoord);
            var diff = endCoord - startCoord;
            return new SquareCoordinateRange()
            {
                coord0 = startCoord,
                rows = diff.row + 1,
                cols = diff.column + 1
            };
        }

        /// <summary>
        /// swap the column and row values of the coords to ensure that coord0 <= coord1 on both axis
        /// </summary>
        private static void EnsureCoordOrdering(ref SquareCoordinate smallCoord, ref SquareCoordinate largerCoord)
        {
            if (smallCoord.column > largerCoord.column)
            {
                var swapSpace = smallCoord.column;
                smallCoord.column = largerCoord.column;
                largerCoord.column = swapSpace;
            }
            if (smallCoord.row > largerCoord.row)
            {
                var swapSpace = smallCoord.row;
                smallCoord.row = largerCoord.row;
                largerCoord.row = swapSpace;
            }
        }
    }
}
