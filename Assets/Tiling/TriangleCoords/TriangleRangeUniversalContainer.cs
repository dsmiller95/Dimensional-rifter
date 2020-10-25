using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    [Serializable]
    public class TriangleRangeUniversalContainer : IUniversalCoordinateRange
    {
        public ICoordinateRangeNEW<TriangleCoordinateStructSystem> baseRange;
        public TriangleRangeUniversalContainer(ICoordinateRangeNEW<TriangleCoordinateStructSystem> baseRange)
        {
            this.baseRange = baseRange;
        }

        public CoordinateType coordinateType => CoordinateType.TRIANGLE;

        public IEnumerable<Vector2> BoundingPolygon()
        {
            return baseRange?.BoundingPolygon();
        }

        public bool ContainsCoordinate(UniversalCoordinate coordinate)
        {
            if (coordinate.type != CoordinateType.TRIANGLE)
            {
                return false;
            }
            return baseRange.ContainsCoordinate(coordinate.triangleDataView);
        }

        public IEnumerable<UniversalCoordinate> GetUniversalCoordinates(short coordPlaneID = 0)
        {
            return baseRange.Select(tri => UniversalCoordinate.From(tri, coordPlaneID));
        }

        public int TotalCoordinateContents()
        {
            return baseRange.TotalCoordinateContents();
        }
    }
}
