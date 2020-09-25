using Assets.Tiling.Tilemapping.NEwSHITE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    /// <summary>
    /// represents a range of triangular coordinates in a rhombus shape. Ignores the R of the input range
    ///     iterates through the triangles as if they were rectangular coordinates, and returns
    ///     both R=false and R=true coords for each rhombus
    /// </summary>
    [Serializable]
    public class TriangleRhomboidCoordinateRange : ICoordinateRangeNEW<TriangleCoordinateStructSystem>
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

        public TriangleCoordinateStructSystem coord0;
        public TriangleCoordinateStructSystem coord1;

        IEnumerator<TriangleCoordinateStructSystem> IEnumerable<TriangleCoordinateStructSystem>.GetEnumerator()
        {
            EnsureCoordOrdering();
            for (var u = coord0.u; u < coord1.u; u++)
            {
                for (var v = coord0.v; v < coord1.v; v++)
                {
                    yield return new TriangleCoordinateStructSystem(u, v, false);
                    yield return new TriangleCoordinateStructSystem(u, v, true);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TriangleCoordinateStructSystem>).GetEnumerator();
        }

        public IEnumerable<Vector2> BoundingPolygon()
        {
            var scaling = 2;// individualScale *= 2;
            EnsureCoordOrdering();

            var nextPos = coord0.ToPositionInPlane();
            yield return nextPos - (TriangleCoordinateStructSystem.rBasis * scaling);

            var nextCoord = new TriangleCoordinateStructSystem(coord0.u, coord1.v - 1, false);
            nextPos = nextCoord.ToPositionInPlane();
            yield return nextPos + Vector2.up * TriangleCoordinateStructSystem.rBasis.y * 2 * scaling;

            nextCoord = new TriangleCoordinateStructSystem(coord1.u - 1, coord1.v - 1, true);
            nextPos = nextCoord.ToPositionInPlane();
            yield return nextPos + (TriangleCoordinateStructSystem.rBasis * scaling);

            nextCoord = new TriangleCoordinateStructSystem(coord1.u - 1, coord0.v, true);
            nextPos = nextCoord.ToPositionInPlane();
            yield return nextPos - Vector2.up * TriangleCoordinateStructSystem.rBasis.y * 2 * scaling;
        }

        public bool ContainsCoordinate(TriangleCoordinateStructSystem coordinate)
        {
            return (coordinate.u >= coord0.u && coordinate.u < coord1.u) &&
                (coordinate.v >= coord0.v && coordinate.v < coord1.v);
        }
    }
}
