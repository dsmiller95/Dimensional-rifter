using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    [Serializable]
    public class SquareCoordinateRange : ICoordinateRange<SquareCoordinate>
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

        IEnumerator<SquareCoordinate> IEnumerable<SquareCoordinate>.GetEnumerator()
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

        public IEnumerable<Vector2> BoundingPolygon(ICoordinateSystem<SquareCoordinate> coordinateSystem, float individualScale)
        {
            var halfScale = individualScale / 2;

            var nextPos = coordinateSystem.ToRealPosition(coord0);
            yield return nextPos - Vector2.one * halfScale;

            var nextCoord = new SquareCoordinate(coord1.row - 1, coord0.column);
            nextPos = coordinateSystem.ToRealPosition(nextCoord);
            yield return nextPos + new Vector2(-1, 1) * halfScale;

            nextCoord = new SquareCoordinate(coord1.row - 1, coord1.column - 1);
            nextPos = coordinateSystem.ToRealPosition(nextCoord);
            yield return nextPos + Vector2.one * halfScale;

            nextCoord = new SquareCoordinate(coord0.row, coord1.column - 1);
            nextPos = coordinateSystem.ToRealPosition(nextCoord);
            yield return nextPos + new Vector2(1, -1) * halfScale;
        }
    }
}
