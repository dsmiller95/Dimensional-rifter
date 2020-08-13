using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.TriangleCoords;
using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    }

    public float movementSpeed = .5f;
    private float lastMove;

    private IList<ICoordinate> currentPath;
    public TileMapMemeber currentTarget;
    public bool SeekClosestOfType(Func<TileMapMemeber, bool> filter)
    {
        if (lastMove + movementSpeed > Time.time)
        {
            return false;
        }

        lastMove = Time.time;
        if (currentTarget == null || currentPath?.Count <= 0)
        {
            currentTarget = null;
            var (bestMemeber, bestPath) = GetPathToClosestOfType(filter);

            if (bestMemeber != null)
            {
                currentPath = bestPath.ToList();
                currentTarget = bestMemeber;
            }
            else
            {
                return false;
            }
        }

        var nextPosition = currentPath[0];
        currentPath.RemoveAt(0);
        SetPosition(nextPosition, homeRegion.UntypedCoordianteSystemWorldSpace);
        if (currentPath.Count <= 0)
        {
            return true;
        }

        return false;
    }
    public (TileMapMemeber, ICoordinate[]) GetPathToClosestOfType(Func<TileMapMemeber, bool> filter)
    {
        var possibleSelections = homeRegion.universalContentTracker.allMembers
            .Where(filter);
        var paths = possibleSelections.Select(member =>
            new
            {
                path = UniversalToGenericAdaptors.PathBetween(
                    coordinatePosition,
                    member.coordinatePosition,
                    homeRegion)?.ToArray(),
                member
            })
            .Where(x => x.path != null);

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

        if (bestMemeber != null)
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
        if (currentPath == null)
        {
            return;
        }
        Gizmos.color = Color.magenta;
        foreach (var pair in currentPath
            .Select(coord => UniversalToGenericAdaptors.ToRealPosition(coord, homeRegion.UntypedCoordianteSystemWorldSpace))
            .RollingWindow(2))
        {
            Gizmos.DrawLine(pair[0], pair[1]);
        }
    }
}
