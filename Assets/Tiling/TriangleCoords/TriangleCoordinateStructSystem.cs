﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct TriangleCoordinateStructSystem : IBaseCoordinateType, IEquatable<TriangleCoordinateStructSystem>
    {
        [FieldOffset(0)]
        public int u;
        [FieldOffset(4)]
        public int v;
        [FieldOffset(8)]
        public bool R;

        public static readonly Vector2 uBasis = new Vector2(1, 0);
        public static readonly Vector2 vBasis = new Vector2(0.5f, Mathf.Sqrt(3) / 2f);

        /// <summary>
        /// inverse matrix of u and v basis, first coord being u. Used to transform from x - y space to u - v space
        /// </summary>
        private static readonly Vector2 xBasis = new Vector2(1, 0);
        private static readonly Vector2 yBasis = new Vector2(-1f / Mathf.Sqrt(3), 2f / Mathf.Sqrt(3));

        public static readonly Vector2 rBasis = new Vector2(0.5f, 1 / (Mathf.Sqrt(3) * 2f)) / 2;

        public TriangleCoordinateStructSystem(int u, int v, bool R)
        {
            this.u = u;
            this.v = v;
            this.R = R;
        }

        public static TriangleCoordinateStructSystem FromPositionInPlane(Vector2 positionInPlane)
        {
            var xComponent = positionInPlane.x * xBasis;
            var yComponent = positionInPlane.y * yBasis;
            var coordCenter = xComponent + yComponent;
            var roundedPoint = new TriangleCoordinateStructSystem(
                Mathf.RoundToInt(coordCenter.x),
                Mathf.RoundToInt(coordCenter.y),
                false);

            var relativePosInSquare = new Vector2(coordCenter.x - roundedPoint.u, coordCenter.y - roundedPoint.v);

            roundedPoint.R = relativePosInSquare.x + relativePosInSquare.y > 0;

            return roundedPoint;
        }
        public Vector2 ToPositionInPlane()
        {
            var uComponent = u * uBasis;
            var vComponent = v * vBasis;
            var realCoord = uComponent + vComponent;
            realCoord += (R ? 1 : -1) * rBasis;
            return realCoord;
        }

        public IEnumerable<TriangleCoordinateStructSystem> Neighbors()
        {
            if (R)
            {
                yield return new TriangleCoordinateStructSystem(u + 1, v, false);
                yield return new TriangleCoordinateStructSystem(u, v, false);
                yield return new TriangleCoordinateStructSystem(u, v + 1, false);
            }
            else
            {
                yield return new TriangleCoordinateStructSystem(u, v, true);
                yield return new TriangleCoordinateStructSystem(u, v - 1, true);
                yield return new TriangleCoordinateStructSystem(u - 1, v, true);
            }
        }

        public static float HeuristicDistance(TriangleCoordinateStructSystem origin, TriangleCoordinateStructSystem destination)
        {
            return (origin.ToPositionInPlane() - destination.ToPositionInPlane()).sqrMagnitude;
        }

        public static readonly Vector2[] triangleVerts = new Vector2[] {
                new Vector3(-.5f,-1/(Mathf.Sqrt(3) * 2)),
                new Vector3(  0f, 1/Mathf.Sqrt(3)),
                new Vector3( .5f, -1/(Mathf.Sqrt(3) * 2)) };


        /// <summary>
        /// Get a list of vertexes representing the triangle around this coordinate,
        ///     with a side length of <paramref name="sideLength"/>
        /// </summary>
        /// <param name="sideLength">the side length of the triangle</param>
        /// <returns>an enumerable of all the vertexes</returns>
        public IEnumerable<Vector2> GetTriangleVertextesAround()
        {
            IEnumerable<Vector2> verts = triangleVerts;
            if (R)
            {
                var rotation = Quaternion.Euler(0, 0, -60);
                verts = verts.Select(x => (Vector2)(rotation * x));
            }
            return verts;
        }

        private static Vector3 BoundBoxSizeEstimate = Vector3.one * 2f / Mathf.Sqrt(3);
        public Bounds GetRawBounds(float sideLength, Matrix4x4 systemTransform)
        {
            var position = systemTransform.MultiplyPoint3x4(ToPositionInPlane());
            return new Bounds(position, BoundBoxSizeEstimate * sideLength);
        }
        public static int[] GetTileTriangleIDs()
        {
            return new int[]
            {
                0, 1, 2
            };
        }

        public static TriangleCoordinateStructSystem AtOrigin()
        {
            return new TriangleCoordinateStructSystem(0, 0, false);
        }

        public static TriangleCoordinateStructSystem operator +(TriangleCoordinateStructSystem a, TriangleCoordinateStructSystem b)
        {
            return new TriangleCoordinateStructSystem(a.u + b.u, a.v + b.v, a.R || b.R);
        }

        public override int GetHashCode()
        {
            var coords = (u << 16) ^ (v);
            if (R)
            {
                return coords ^ (1 << 31);
            }
            return coords;
        }
        public override string ToString()
        {
            return $"u: {u}, v: {v}, R: {R}";
        }

        public override bool Equals(object obj)
        {
            if (obj is TriangleCoordinateStructSystem coord)
            {
                return Equals(coord);
            }
            return false;
        }
        public bool Equals(TriangleCoordinateStructSystem coord)
        {
            return coord.R == R && coord.u == u && coord.v == v;
        }
    }
}
