using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling
{
    public interface ICoordinateSystem<T> where T : ICoordinate
    {
        IEnumerable<T> Neighbors(T coordinate);
        Vector2 ToRealPosition(T coordinate);
        T FromRealPosition(Vector2 realWorldPos);
    }

    public interface ICoordinate { }

    public interface ICoordinateRange<T>: IEnumerable<T> where T : ICoordinate
    { }
}
