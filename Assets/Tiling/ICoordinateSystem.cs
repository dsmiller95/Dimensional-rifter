using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling
{
    public interface ITileMapSystem<T> where T : ICoordinate
    {
        Bounds GetRawBounds(T coord, float sideLength, ICoordinateSystem<T> translateCoordinateSystem);

        IEnumerable<Vector2> GetVertexesAround(T coord, float sideLength, ICoordinateSystem<T> translateCoordinateSystem = null);

        int[] GetTileTriangles();

        ICoordinateSystem<T> GetBasicCoordinateSystem();
    }

    public interface ICoordinateSystem<T> where T : ICoordinate
    {
        IEnumerable<T> Neighbors(T coordinate);
        Vector2 ToRealPosition(T coordinate);
        T FromRealPosition(Vector2 realWorldPos);

        T DefaultCoordinate();
    }

    public interface ICoordinate { }

    public interface ICoordinateRange<T>: IEnumerable<T> where T : ICoordinate
    {
        /// <summary>
        /// Generate a collider for this 
        /// </summary>
        /// <returns></returns>
        IEnumerable<Vector2> BoundingPolygon(ICoordinateSystem<T> coordinateSystem, float individualScale);
    }
}
