using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    [Serializable]
    public class SquareRangeUniversalContainer : IUniversalCoordinateRange
    {
        public ICoordinateRangeNEW<SquareCoordinate> baseRange;
        public SquareRangeUniversalContainer(ICoordinateRangeNEW<SquareCoordinate> baseRange)
        {
            this.baseRange = baseRange;
        }

        public CoordinateType coordinateType => CoordinateType.SQUARE;

        public IEnumerable<Vector2> BoundingPolygon()
        {
            return baseRange?.BoundingPolygon();
        }

        public bool ContainsCoordinate(UniversalCoordinate coordinate)
        {
            if (coordinate.type != CoordinateType.SQUARE)
            {
                return false;
            }
            return baseRange.ContainsCoordinate(coordinate.squareDataView);
        }

        public IEnumerable<UniversalCoordinate> GetUniversalCoordinates(short coordPlaneID = 0)
        {
            return baseRange.Select(square => UniversalCoordinate.From(square, coordPlaneID));
        }
    }
}
