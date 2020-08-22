using Assets.Tiling;
using Assets.WorldObjects;
using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NavigationAttemptResult
{
    ARRIVED,
    NO_TARGETS,
    APPROACHING
}

/// <summary>
/// Provides methods to navigate through a tileMap
///     stores information about the current location in the tileMap
///     
/// </summary>
public class TileMapNavigationMember : TileMapMember
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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
    public TileMapMember currentTarget;
    public NavigationAttemptResult SeekClosestOfType(Func<TileMapMember, bool> filter)
    {
        if (lastMove + movementSpeed > Time.time)
        {
            return NavigationAttemptResult.APPROACHING;
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
                return NavigationAttemptResult.NO_TARGETS;
            }
        }
        if (currentPath.Count <= 0)
        {
            // we got there without needing to move at all
            return NavigationAttemptResult.ARRIVED;
        }

        var nextPosition = currentPath[0];
        currentPath.RemoveAt(0);
        SetPosition(nextPosition, homeRegion.UntypedCoordianteSystemWorldSpace);
        if (currentPath.Count <= 0)
        {
            return NavigationAttemptResult.ARRIVED;
        }

        return NavigationAttemptResult.APPROACHING;
    }
    public (TileMapMember, ICoordinate[]) GetPathToClosestOfType(Func<TileMapMember, bool> filter)
    {
        var possibleSelections = homeRegion.universalContentTracker.allMembers
            .Where(filter);
        var paths = possibleSelections.Select(member =>
            new
            {
                path = PathfinderUtils.PathBetween(
                    coordinatePosition,
                    member.CoordinatePosition,
                    homeRegion,
                    properties => properties.isPassable)?.ToArray(),
                member
            })
            .Where(x => x.path != null);

        var minDist = float.MaxValue;
        ICoordinate[] bestPath = new ICoordinate[0];
        TileMapMember bestMemeber = null;

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
