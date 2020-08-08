using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    [Serializable]
    public struct TriangleCoordinate : ICoordinate
    {
        public TriangleCoordinate(int u, int v, bool R)
        {
            this.u = u;
            this.v = v;
            this.R = R;
        }
        public int u;
        public int v;
        public bool R;

        public IEnumerable<TriangleCoordinate> Neighbors()
        {
            if (R)
            {
                yield return new TriangleCoordinate(u + 1, v, false);
                yield return new TriangleCoordinate(u, v, true);
                yield return new TriangleCoordinate(u, v + 1, false);
            }
            else
            {
                yield return new TriangleCoordinate(u, v, false);
                yield return new TriangleCoordinate(u, v - 1, true);
                yield return new TriangleCoordinate(u - 1, v, true);
            }
        }

        public override string ToString()
        {
            return $"u: {u}, v: {v}, R: {R}";
        }
    }


    /// <summary>
    /// Represents a coordinate system of squares of side length 1
    /// </summary>
    public class TriangleCoordinateSystem : ICoordinateSystem<TriangleCoordinate>
    {
        private static readonly Vector2 uBasis = new Vector2(1, 0);
        private static readonly Vector2 vBasis = new Vector2(0.5f, Mathf.Sqrt(3) / 2f);

        /// <summary>
        /// inverse matrix of u and v basis, first coord being u. Used to transform from x - y space to u - v space
        /// </summary>
        private static readonly Vector2 xBasis = new Vector2(1, 0);
        private static readonly Vector2 yBasis = new Vector2(-1f / Mathf.Sqrt(3), 2f / Mathf.Sqrt(3));

        private static readonly Vector2 rBasis = new Vector2(0.5f, 1 / (Mathf.Sqrt(3) * 2f)) / 2;

        public TriangleCoordinate FromRealPosition(Vector2 realWorldPos)
        {
            var xComponent = realWorldPos.x * xBasis;
            var yComponent = realWorldPos.y * yBasis;
            var coordCenter = xComponent + yComponent;
            var roundedPoint = new TriangleCoordinate(Mathf.RoundToInt(coordCenter.x), Mathf.RoundToInt(coordCenter.y), false);

            var relativePosInSquare = new Vector2(coordCenter.x - roundedPoint.u, coordCenter.y - roundedPoint.v);

            roundedPoint.R = relativePosInSquare.x + relativePosInSquare.y > 0;

            return roundedPoint;
        }

        public IEnumerable<TriangleCoordinate> Neighbors(TriangleCoordinate coordinate)
        {
            return coordinate.Neighbors();
        }

        public Vector2 ToRealPosition(TriangleCoordinate coordinate)
        {
            var uComponent = coordinate.u * uBasis;
            var vComponent = coordinate.v * vBasis;
            var realCoord = uComponent + vComponent;
            realCoord += (coordinate.R ? 1 : -1) * rBasis;
            return realCoord;
        }
    }
}
