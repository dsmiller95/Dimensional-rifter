using System;
using System.Collections;
using System.Collections.Generic;

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

        public SquareCoordinate coord0;
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
    }
}
