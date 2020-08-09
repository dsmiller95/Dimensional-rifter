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
    [System.Serializable]
    public class TriangleTriangleCoordinateRange : ICoordinateRange<TriangleCoordinate>
    {
        public TriangleCoordinate root;
        public int triangleSideLength;

        public IEnumerable<Vector2> BoundingPolygon(ICoordinateSystem<TriangleCoordinate> coordinateSystem, float individualScale)
        {
            individualScale *= 2;

            var nextPos = coordinateSystem.ToRealPosition(root);
            yield return nextPos - (TriangleCoordinateSystem.rBasis * individualScale);

            var topCoord = new TriangleCoordinate(root.u, root.v + triangleSideLength - 1, false);
            nextPos = coordinateSystem.ToRealPosition(topCoord);
            yield return nextPos + Vector2.up * TriangleCoordinateSystem.rBasis.y * 2 * individualScale;

            var rightcoord = new TriangleCoordinate(root.u + triangleSideLength - 1, root.v, false);
            nextPos = coordinateSystem.ToRealPosition(rightcoord);
            yield return nextPos + Vector2.Scale(new Vector2(1, -1), TriangleCoordinateSystem.rBasis * individualScale);
        }

        IEnumerator<TriangleCoordinate> IEnumerable<TriangleCoordinate>.GetEnumerator()
        {
            for (var u = 0; u < triangleSideLength; u++)
            {
                for (var v = 0; v < triangleSideLength; v++)
                {
                    if (u + v < triangleSideLength - 1)
                    {
                        yield return new TriangleCoordinate(u + root.u, v + root.v, false);
                        yield return new TriangleCoordinate(u + root.u, v + root.v, true);
                    }
                    else if (u + v < triangleSideLength)
                    {
                        yield return new TriangleCoordinate(u + root.u, v + root.v, false);
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TriangleCoordinate>).GetEnumerator();
        }
    }
}
