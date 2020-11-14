using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{

    /// <summary>
    /// represents a range of triangular coordinates in a rhombus shape. Ignores the R of the input range
    ///     iterates through the triangles as if they were rectangular coordinates, and returns
    ///     both R=false and R=true coords for each rhombus
    /// </summary>
    [System.Serializable]
    [StructLayout(LayoutKind.Explicit)] // total size: 16 bytes
    public struct TriangleTriangleCoordinateRange : ICoordinateRange<TriangleCoordinateStructSystem>, IEquatable<TriangleTriangleCoordinateRange>
    {
        [FieldOffset(0)] public TriangleCoordinateStructSystem root;
        [FieldOffset(12)] public int triangleSideLength;

        public IEnumerable<Vector2> BoundingPolygon()
        {
            var scale = 2;

            var nextPos = root.ToPositionInPlane();
            yield return nextPos - (TriangleCoordinateStructSystem.rBasis * scale);

            var topCoord = new TriangleCoordinateStructSystem(root.u, root.v + triangleSideLength - 1, false);
            nextPos = topCoord.ToPositionInPlane();
            yield return (Vector2)nextPos + Vector2.up * TriangleCoordinateStructSystem.rBasis.y * 2 * scale;

            var rightcoord = new TriangleCoordinateStructSystem(root.u + triangleSideLength - 1, root.v, false);
            nextPos = rightcoord.ToPositionInPlane();
            yield return (Vector2)nextPos + Vector2.Scale(new Vector2(1, -1), TriangleCoordinateStructSystem.rBasis * scale);
        }


        IEnumerator<TriangleCoordinateStructSystem> IEnumerable<TriangleCoordinateStructSystem>.GetEnumerator()
        {
            for (var u = 0; u < triangleSideLength; u++)
            {
                for (var v = 0; v < triangleSideLength; v++)
                {
                    if (u + v < triangleSideLength - 1)
                    {
                        yield return new TriangleCoordinateStructSystem(u + root.u, v + root.v, false);
                        yield return new TriangleCoordinateStructSystem(u + root.u, v + root.v, true);
                    }
                    else if (u + v < triangleSideLength)
                    {
                        yield return new TriangleCoordinateStructSystem(u + root.u, v + root.v, false);
                    }
                }
            }
        }
        public bool ContainsCoordinate(UniversalCoordinate universalCoordinate)
        {
            if (universalCoordinate.type != CoordinateType.TRIANGLE)
            {
                return false;
            }
            return ContainsCoordinate(universalCoordinate.triangleDataView);
        }
        public bool ContainsCoordinate(TriangleCoordinateStructSystem coordinat)
        {
            var uConst = coordinat.u - root.u;
            var vConst = coordinat.v - root.v;
            var constConst = uConst + vConst + (coordinat.R ? 0 : -1);
            return uConst >= 0 && vConst >= 0 && constConst < triangleSideLength;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TriangleCoordinateStructSystem>).GetEnumerator();
        }

        public int TotalCoordinateContents()
        {
            return triangleSideLength * triangleSideLength;
        }

        public bool Equals(TriangleTriangleCoordinateRange other)
        {
            return root.Equals(other.root) && triangleSideLength == other.triangleSideLength;
        }
    }
}
