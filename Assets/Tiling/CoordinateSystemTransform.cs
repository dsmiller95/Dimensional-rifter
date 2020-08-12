using Assets.Tiling;
using System.Collections.Generic;
using UnityEngine;


public class CoordinateSystemTransform<T> : ICoordinateSystem<T> where T : ICoordinate
{
    private ICoordinateSystem<T> basis;

    private Transform transform;

    public CoordinateSystemType CoordType => basis.CoordType;

    public CoordinateSystemTransform(ICoordinateSystem<T> basis, Transform transform)
    {
        this.basis = basis;
        this.transform = transform;
    }

    public T DefaultCoordinate()
    {
        return basis.DefaultCoordinate();
    }

    public T FromRealPosition(Vector2 realWorldPos)
    {
        var transformedPos = transform.InverseTransformPoint(realWorldPos);
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
        return transform.TransformPoint(localPoint);
    }
}
