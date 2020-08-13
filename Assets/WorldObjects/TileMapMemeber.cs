using Assets.Tiling;
using Assets.Tiling.Tilemapping.Triangle;
using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets;
using System;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping.Square;

/// <summary>
/// Provides methods to navigate through a tileMap
///     stores information about the current location in the tileMap
///     
/// </summary>
public class TileMapMemeber : MonoBehaviour
{
    public TileMapRegionNoCoordinateType homeRegion;

    public SquareCoordinate position;
    //public TriangleCoordinateSystemBehavior coordinateSystemForInspector;
    //public TriangleCoordinate position;

    private ICoordinate coordinatePosition;
    private object coordinateSystem;

    public void SetPosition(ICoordinate position, ICoordinateSystem coordinateSystem)
    {
        coordinatePosition = position;
        this.coordinateSystem = coordinateSystem;

        transform.position = UniversalToGenericAdaptors.ToRealPosition(position, coordinateSystem);
    }



    // Start is called before the first frame update
    void Start()
    {
        SetPosition(position, homeRegion.UntypedCoordianteSystemWorldSpace);
        //homeRegion.
        lastMove = Time.time;
        homeRegion.universalContentTracker.RegisterInTileMap(this);
    }


    // Update is called once per frame
    void Update()
    {
        //var position = (TriangleCoordinate)coordinatePosition;
        //var system = coordinateSystem as ICoordinateSystem<TriangleCoordinate>;
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    var mousePos = MyUtilities.GetMousePos2D();
        //    var coords = system.FromRealPosition(mousePos);
        //    UpdatePath(position, system, coords);
        //}
        //if (lastMove + movementSpeed < Time.time)
        //{
        //    lastMove = Time.time;
        //    if(path == null || path.Count <= 0)
        //    {
        //        return;
        //    }
        //    var nextPosition = path[0];
        //    path.RemoveAt(0);
        //    //var nextOptions = system.Neighbors(position).ToArray();
        //    //var nextPosition = nextOptions[Random.Range(0, nextOptions.Length)];

        //    SetPosition(nextPosition, system);
        //    //UpdatePath(nextPosition, system);
        //}
    }

    public float movementSpeed = .5f;
    private float lastMove;

    private IList<TriangleCoordinate> path;
    private IList<ICoordinate> currentPath;
    public TileMapMemeber currentTarget;
    public bool SeekClosestOfType(Func<TileMapMemeber, bool> filter)
    {
        if(currentTarget == null || path?.Count <= 0)
        {
            currentTarget = null;
            var (bestMemeber, bestPath) = this.GetPathToClosestOfType(filter);

            if (bestMemeber != null)
            {
                currentPath = bestPath.ToList();
                currentTarget = bestMemeber;
            }else
            {
                return false;
            }
        }
        if (lastMove + movementSpeed < Time.time)
        {
            lastMove = Time.time;

            var nextPosition = currentPath[0];
            currentPath.RemoveAt(0);
            SetPosition(nextPosition, homeRegion.UntypedCoordianteSystemWorldSpace);


            if (currentPath.Count <= 0)
            {
                return true;
            }
        }


        return false;
    }
    public (TileMapMemeber, ICoordinate[]) GetPathToClosestOfType(Func<TileMapMemeber, bool> filter)
    {
        var possibleSelections = homeRegion.universalContentTracker.allMembers
            .Where(filter);
        var paths = possibleSelections.Select(member =>
        new {
            path = UniversalToGenericAdaptors.PathBetween(this.coordinatePosition, member.coordinatePosition, homeRegion.UntypedCoordianteSystemWorldSpace).ToArray(),
            member
        });

        var minDist = float.MaxValue;
        ICoordinate[] bestPath = new ICoordinate[0];
        TileMapMemeber bestMemeber = null;

        foreach (var path in paths)
        {
            if (path.path.Length < minDist)
            {
                bestPath = path.path;
                minDist = bestPath.Length;
                bestMemeber = path.member;
            }
        }

        if(bestMemeber != null)
        {
            currentPath = bestPath.ToList();
            currentTarget = bestMemeber;

        }
        return (bestMemeber, bestPath);
    }

    private void OnDestroy()
    {
        homeRegion.universalContentTracker.DeRegisterInTileMap(this);
    }


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
