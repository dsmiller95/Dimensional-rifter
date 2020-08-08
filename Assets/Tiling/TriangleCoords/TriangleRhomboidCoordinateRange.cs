using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Assets.Tiling.TriangleCoords
{

    /// <summary>
    /// represents a range of triangular coordinates in a rhombus shape. Ignores the R of the input range
    ///     iterates through the triangles as if they were rectangular coordinates, and returns
    ///     both R=false and R=true coords for each rhombus
    /// </summary>
    [Serializable]
    public class TriangleRhomboidCoordinateRange : ICoordinateRange<TriangleCoordinate>
    {
        /// <summary>
        /// swap the column and row values of the coords to ensure that coord0 <= coord1 on both axis
        /// </summary>
        private void EnsureCoordOrdering()
        {
            if (coord0.u > coord1.u)
            {
                var swapSpace = coord0.u;
                coord0.u = coord1.u;
                coord1.u = swapSpace;
            }
            if (coord0.v > coord1.v)
            {
                var swapSpace = coord0.v;
                coord0.v = coord1.v;
                coord1.v = swapSpace;
            }
        }

        public TriangleCoordinate coord0;
        public TriangleCoordinate coord1;

        IEnumerator<TriangleCoordinate> IEnumerable<TriangleCoordinate>.GetEnumerator()
        {
            EnsureCoordOrdering();
            for (var u = coord0.u; u < coord1.u; u++)
            {
                for (var v = coord0.v; v < coord1.v; v++)
                {
                    yield return new TriangleCoordinate(u, v, false);
                    yield return new TriangleCoordinate(u, v, true);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TriangleCoordinate>).GetEnumerator();
        }
    }
}
