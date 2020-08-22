﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.SquareCoords
{
    /// <summary>
    /// Represents a coordinate system of squares of side length 1. This system makes the assumption that it is not rotated to save time on frequently
    ///     used calls
    /// </summary>
    [CreateAssetMenu(fileName = "SquareTilingSystem", menuName = "TileMap/SquareTilingSystem", order = 1)]
    public class SquareNonRotatedTileMapSystem : TileMapTileShapeStrategy<SquareCoordinate>
    {
        public override ICoordinateSystem<SquareCoordinate> GetBasicCoordinateSystem()
        {
            return new SquareCoordinateSystem();
        }

        public override int[] GetTileTriangles()
        {
            return new int[]
            {
                0, 1, 2,
                2, 3, 0
            };
        }

        public override Bounds GetRawBounds(SquareCoordinate coord, float sideLength, ICoordinateSystem<SquareCoordinate> translateCoordinateSystem)
        {
            var position = translateCoordinateSystem.ToRealPosition(coord);
            return new Bounds(position, Vector3.one * sideLength);
        }

        public override IEnumerable<Vector2> GetVertexesAround(SquareCoordinate coord, float sideLength, ICoordinateSystem<SquareCoordinate> translateCoordinateSystem)
        {
            return SquareCoordinateSystem.GetSquareVertsAround(coord, sideLength, translateCoordinateSystem);
        }
    }
}