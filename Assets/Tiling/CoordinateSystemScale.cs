using Assets;
using Assets.Tiling;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// simplified version of a <see cref="CoordinateSystemTransform{T}"/> which only scales using a Vector2
/// </summary>
/// <typeparam name="T"></typeparam>
public class CoordinateSystemScale<T> : ICoordinateSystem<T> where T : ICoordinate
{
    private ICoordinateSystem<T> basis;

    private Vector2 scale;

    public CoordinateSystemType CoordType => basis.CoordType;
    public CoordinateSystemScale(ICoordinateSystem<T> basis, Vector2 scale)
    {
        this.basis = basis;
        this.scale = scale;
    }

    public T DefaultCoordinate()
    {
        return basis.DefaultCoordinate();
    }

    public T FromRealPosition(Vector2 realWorldPos)
    {
        var transformedPos = realWorldPos.InverseScale(scale);
        return basis.FromRealPosition(transformedPos);
    }

    public float HeuristicDistance(T origin, T destination)
    {
        return basis.HeuristicDistance(origin, destination);
    }

    public IEnumerable<T> Neighbors(T coordinate)
    {
        return basis.Neighbors(coordinate);
    }

    public Vector2 ToRealPosition(T coordinate)
    {
        var localPoint = basis.ToRealPosition(coordinate);
        return Vector2.Scale(localPoint, scale);
    }
}
