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
    [Serializable]
    [StructLayout(LayoutKind.Explicit)] // total size: 20 bytes
    public struct TriangleRhomboidCoordinateRange : ICoordinateRange<TriangleCoordinateStructSystem>, IEquatable<TriangleRhomboidCoordinateRange>
    {
        /// <summary>
        /// swap the column and row values of the coords to ensure that coord0 <= coord1 on both axis
        /// </summary>
        private static void EnsureCoordOrdering(ref TriangleCoordinateStructSystem smallCoord, ref TriangleCoordinateStructSystem largerCoord)
        {
            if (smallCoord.u > largerCoord.u)
            {
                var swapSpace = smallCoord.u;
                smallCoord.u = largerCoord.u;
                largerCoord.u = swapSpace;
            }
            if (smallCoord.v > largerCoord.v)
            {
                var swapSpace = smallCoord.v;
                smallCoord.v = largerCoord.v;
                largerCoord.v = swapSpace;
            }
        }
        public static TriangleRhomboidCoordinateRange FromCoordsLargestExclusive(TriangleCoordinateStructSystem startCoord, TriangleCoordinateStructSystem endCoord)
        {
            EnsureCoordOrdering(ref startCoord, ref endCoord);
            return new TriangleRhomboidCoordinateRange()
            {
                coord0 = startCoord,
                uSize = endCoord.u - startCoord.u,
                vSize = endCoord.v - startCoord.v
            };
        }

        [FieldOffset(0)] public TriangleCoordinateStructSystem coord0;
        [FieldOffset(12)] public int uSize;
        [FieldOffset(16)] public int vSize;

        IEnumerator<TriangleCoordinateStructSystem> IEnumerable<TriangleCoordinateStructSystem>.GetEnumerator()
        {
            for (var u = 0; u < uSize; u++)
            {
                for (var v = 0; v < vSize; v++)
                {
                    yield return new TriangleCoordinateStructSystem(u + coord0.u, v + coord0.v, false);
                    yield return new TriangleCoordinateStructSystem(u + coord0.u, v + coord0.v, true);
                }
            }
        }

        public TriangleCoordinateStructSystem AtIndex(int index)
        {
            var resultStruct = new TriangleCoordinateStructSystem();
            resultStruct.R = index % 2 == 1;
            var halfIndex = index / 2;
            resultStruct.u = (halfIndex / vSize);
            resultStruct.v = (halfIndex % vSize);

            resultStruct.u += coord0.u;
            resultStruct.v += coord0.v;
            return resultStruct;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TriangleCoordinateStructSystem>).GetEnumerator();
        }

        public IEnumerable<Vector2> BoundingPolygon()
        {
            var scaling = 2;// individualScale *= 2;

            var nextPos = coord0.ToPositionInPlane();
            yield return nextPos - (TriangleCoordinateStructSystem.rBasis * scaling);

            var nextCoord = new TriangleCoordinateStructSystem(coord0.u, coord0.v + vSize - 1, false);
            nextPos = nextCoord.ToPositionInPlane();
            yield return (Vector2)nextPos + Vector2.up * TriangleCoordinateStructSystem.rBasis.y * 2 * scaling;

            nextCoord = new TriangleCoordinateStructSystem(coord0.u + uSize - 1, coord0.v + vSize - 1, true);
            nextPos = nextCoord.ToPositionInPlane();
            yield return nextPos + (TriangleCoordinateStructSystem.rBasis * scaling);

            nextCoord = new TriangleCoordinateStructSystem(coord0.u + uSize - 1, coord0.v, true);
            nextPos = nextCoord.ToPositionInPlane();
            yield return (Vector2)nextPos - Vector2.up * TriangleCoordinateStructSystem.rBasis.y * 2 * scaling;
        }

        public bool ContainsCoordinate(UniversalCoordinate universalCoordinate)
        {
            if (universalCoordinate.type != CoordinateType.TRIANGLE)
            {
                return false;
            }
            return ContainsCoordinate(universalCoordinate.triangleDataView);
        }
        public bool ContainsCoordinate(TriangleCoordinateStructSystem coordinate)
        {
            var uDiff = coordinate.u - coord0.u;
            if(!(uDiff >= 0 && uDiff < uSize))
            {
                return false;
            }
            var vDiff = coordinate.v - coord0.v;
            return vDiff >= 0 && vDiff < vSize;
        }
        public int TotalCoordinateContents()
        {
            return (uSize) * (vSize) * 2;
        }

        public bool Equals(TriangleRhomboidCoordinateRange other)
        {
            return coord0.Equals(other.coord0) && uSize == other.uSize && vSize == other.vSize;
        }
    }
}
