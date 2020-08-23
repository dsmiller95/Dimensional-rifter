﻿using Assets.Tiling;
using Assets.WorldObjects;
using Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public enum NavigationStatus
{
    ARRIVED,
    INVALID_TARGET,
    APPROACHING
}

public struct NavigationPath
{
    public IList<ICoordinate> coordinatePath;
    public TileMapMember targetMember;
}

public struct NavigationAttemptResult
{
    public NavigationStatus status;
    public TileMapMember reached;
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
    }


    // Update is called once per frame
    void Update()
    {
    }

    public float movementSpeed = .5f;
    private float lastMove;

    private NavigationPath currentPath;
    public NavigationAttemptResult SeekClosestOfType(Func<TileMapMember, bool> filter)
    {
        var navigationResult = AttemptAdvanceAlongCurrentPath();
        if(navigationResult.status == NavigationStatus.INVALID_TARGET && BeginTrackingPathToClosestOfType(filter))
        {
            return new NavigationAttemptResult
            {
                status = NavigationStatus.APPROACHING
            };
        }
        return navigationResult;
    }

    /// <summary>
    /// Attempt to advance the member along the currently set path. Will return INVALID_TARGET if there is no set target, or if 
    ///     the current path is no longer valid. this could happen if the target has been destroyed, or if something impassible has been
    ///     placed along the current path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public NavigationAttemptResult AttemptAdvanceAlongCurrentPath()
    {
        if (lastMove + movementSpeed > Time.time)
        {
            return new NavigationAttemptResult
            {
                status = NavigationStatus.APPROACHING
            };
        }
        lastMove = Time.time;

        if(currentPath.targetMember == null)
        {
            return new NavigationAttemptResult
            {
                status = NavigationStatus.INVALID_TARGET
            };
        }
        if(currentPath.coordinatePath.Count <= 0)
        {
            var result = new NavigationAttemptResult
            {
                status = NavigationStatus.ARRIVED,
                reached = currentPath.targetMember
            };
            currentPath = default;
            return result;
        }

        var nextPosition = currentPath.coordinatePath[0];
        currentPath.coordinatePath.RemoveAt(0);
        SetPosition(nextPosition, currentRegion);

        return new NavigationAttemptResult
        {
            status = NavigationStatus.APPROACHING
        };
    }

    /// <summary>
    /// Attempt to being pathing to the closest object. will store the path internally if one exists
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>true if a path exists to any member matching <paramref name="filter"/>, false otherwise</returns>
    public bool BeginTrackingPathToClosestOfType(Func<TileMapMember, bool> filter)
    {
        var possibleSelections = currentRegion.universalContentTracker.allMembers
            .Where(filter);
        var paths = possibleSelections.Select(member =>
            new
            {
                path = PathfinderUtils.PathBetween(
                    coordinatePosition,
                    member.CoordinatePosition,
                    currentRegion,
                    (coord, properties) => currentRegion.universalContentTracker.IsPassableTypeUnsafe(coord))?.ToList(),
                member
            })
            .Where(x => x.path != null);

        var minDist = float.MaxValue;
        IList<ICoordinate> bestPath = new ICoordinate[0];
        TileMapMember bestMemeber = null;

        foreach (var path in paths)
        {
            if (path.path.Count < minDist)
            {
                bestPath = path.path;
                minDist = bestPath.Count;
                bestMemeber = path.member;
            }
        }

        if(bestMemeber == null)
        {
            return false;
        }
        currentPath = new NavigationPath
        {
            coordinatePath = bestPath,
            targetMember = bestMemeber
        };
        return true;
    }

    private void OnDestroy()
    {
        currentRegion.universalContentTracker.DeRegisterInTileMap(this);
    }

    private void OnDrawGizmos()
    {
        if (currentPath.coordinatePath == null)
        {
            return;
        }
        Gizmos.color = Color.magenta;
        foreach (var pair in currentPath.coordinatePath
            .Select(coord => UniversalToGenericAdaptors.ToRealPosition(coord, currentRegion.UntypedCoordianteSystemWorldSpace))
            .RollingWindow(2))
        {
            Gizmos.DrawLine(pair[0], pair[1]);
        }
    }
}
