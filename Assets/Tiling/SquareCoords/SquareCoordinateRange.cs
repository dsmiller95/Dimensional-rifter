﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    [Serializable]
    public class SquareCoordinateRange : ICoordinateRangeNEW<SquareCoordinate>
    {
        /// <summary>
        /// swap the column and row values of the coords to ensure that coord0 <= coord1 on both axis
        /// </summary>
        private void EnsureCoordOrdering()
        {
            if (coord0.column > coord1.column)
            {
                var swapSpace = coord0.column;
                coord0.column = coord1.column;
                coord1.column = swapSpace;
            }
            if (coord0.row > coord1.row)
            {
                var swapSpace = coord0.row;
                coord0.row = coord1.row;
                coord1.row = swapSpace;
            }
        }

        /// <summary>
        /// beginning of the range (inclusive)
        /// </summary>
        public SquareCoordinate coord0;
        /// <summary>
        /// end of the range (exclusive)
        /// </summary>
        public SquareCoordinate coord1;

        public IEnumerator<SquareCoordinate> GetEnumerator()
        {
            EnsureCoordOrdering();
            for (var column = coord0.column; column < coord1.column; column++)
            {
                for (var row = coord0.row; row < coord1.row; row++)
                {
                    yield return new SquareCoordinate(row, column);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<SquareCoordinate>).GetEnumerator();
        }

        public IEnumerable<Vector2> BoundingPolygon()
        {
            var halfScale = 1 / 2;

            var nextPos = coord0.ToPositionInPlane();
            yield return nextPos - Vector2.one * halfScale;

            var nextCoord = new SquareCoordinate(coord1.row - 1, coord0.column);
            nextPos = nextCoord.ToPositionInPlane();
            yield return nextPos + new Vector2(-1, 1) * halfScale;

            nextCoord = new SquareCoordinate(coord1.row - 1, coord1.column - 1);
            nextPos = nextCoord.ToPositionInPlane();
            yield return nextPos + Vector2.one * halfScale;

            nextCoord = new SquareCoordinate(coord0.row, coord1.column - 1);
            nextPos = nextCoord.ToPositionInPlane();
            yield return nextPos + new Vector2(1, -1) * halfScale;
        }

        public bool ContainsCoordinate(SquareCoordinate coordinat)
        {
            return (coordinat.column >= coord0.column && coordinat.column < coord1.column) &&
                (coordinat.row >= coord0.row && coordinat.row < coord1.row);
        }

        public override string ToString()
        {
            return $"({coord0})-({coord1})";
        }

        public int TotalCoordinateContents()
        {
            EnsureCoordOrdering();
            var diff = coord1 - coord0;
            return diff.row * diff.column;
        }
    }
}
