using Assets.Tiling;
using Assets.Tiling.Tilemapping.Triangle;
using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets;

/// <summary>
/// Provides methods to navigate through a tileMap
///     stores information about the current location in the tileMap
///     
/// </summary>
public class TileMapMemeber : MonoBehaviour
{
    //public SquareCoordinateSystemBehavior coordinateSystemForInspector;
    public TriangleCoordinateSystemBehavior coordinateSystemForInspector;
    public TriangleCoordinate position;

    private ICoordinate coordinatePosition;
    private object coordinateSystem;

    public void SetPosition<T>(T position, ICoordinateSystem<T> coordinateSystem) where T : ICoordinate
    {
        coordinatePosition = position;
        this.coordinateSystem = coordinateSystem;

        transform.position = coordinateSystem.ToRealPosition(position);
    }


    // Start is called before the first frame update
    void Start()
    {
        SetPosition(position, coordinateSystemForInspector.coordinateSystem);
        lastMove = Time.time;
    }

    public float movementSpeed = .5f;
    private float lastMove;

    // Update is called once per frame
    void Update()
    {
        var position = (TriangleCoordinate)coordinatePosition;
        var system = coordinateSystem as ICoordinateSystem<TriangleCoordinate>;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var mousePos = MyUtilities.GetMousePos2D();
            var coords = system.FromRealPosition(mousePos);
            UpdatePath(position, system, coords);
        }
        if (lastMove + movementSpeed < Time.time)
        {
            lastMove = Time.time;
            if(path == null || path.Count <= 0)
            {
                return;
            }
            var nextPosition = path[0];
            path.RemoveAt(0);
            //var nextOptions = system.Neighbors(position).ToArray();
            //var nextPosition = nextOptions[Random.Range(0, nextOptions.Length)];

            SetPosition(nextPosition, system);
            //UpdatePath(nextPosition, system);
        }
    }



    private IList<TriangleCoordinate> path;

    private void OnDrawGizmos()
    {
        if (path == null)
        {
            return;
        }
        Gizmos.color = Color.magenta;
        var system = coordinateSystem as ICoordinateSystem<TriangleCoordinate>;
        foreach (var pair in path
            .Select(coord => system.ToRealPosition(coord))
            .RollingWindow(2))
        {
            Gizmos.DrawLine(pair[0], pair[1]);
        }
    }

    private void UpdatePath(TriangleCoordinate position, ICoordinateSystem<TriangleCoordinate> coordinateSystem, TriangleCoordinate destination)
    {
        var pather = new Pathfinder<TriangleCoordinate>(destination, coordinateSystem);
        path = pather.ShortestPathTo(position)
            .ToList();
    }
}
