using Assets.Tiling;
using Assets.WorldObjects;
using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects
{
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
            if (navigationResult.status == NavigationStatus.INVALID_TARGET && BeginTrackingPathToClosestOfType(filter))
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
        /// <returns></returns>
        public NavigationAttemptResult AttemptAdvanceAlongCurrentPath()
        {
            var result = AttemptAdvanceAlongPath(currentPath);

            if (result == NavigationStatus.ARRIVED)
            {
                var wrappedResult = new NavigationAttemptResult
                {
                    reached = currentPath.targetMember,
                    status = result
                };
                currentPath = default;
                return wrappedResult;
            }
            if (result == NavigationStatus.INVALID_TARGET)
            {
                currentPath = default;
            }
            return new NavigationAttemptResult
            {
                status = result
            };
        }
        /// <summary>
        /// Attempt to advance the member along the given path. Will return INVALID_TARGET if there is no set target, or if 
        ///     the current path is no longer valid. this could happen if the target has been destroyed, or if something impassible has been
        ///     placed along the current path
        /// </summary>
        /// <param name="path">the path to advance along</param>
        /// <returns></returns>
        public NavigationStatus AttemptAdvanceAlongPath(NavigationPath path)
        {
            if (lastMove + movementSpeed > Time.time)
            {
                return NavigationStatus.APPROACHING;
            }
            lastMove = Time.time;

            if (path.targetMember == null)
            {
                return NavigationStatus.INVALID_TARGET;
            }
            if (path.coordinatePath.Count <= 0)
            {
                return NavigationStatus.ARRIVED;
            }

            var nextPosition = path.coordinatePath[0];
            path.coordinatePath.RemoveAt(0);
            SetPosition(nextPosition, currentRegion);

            return NavigationStatus.APPROACHING;
        }

        /// <summary>
        /// Attempt to being pathing to the closest object. will store the path internally if one exists
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>true if a path exists to any member matching <paramref name="filter"/>, false otherwise</returns>
        public bool BeginTrackingPathToClosestOfType(Func<TileMapMember, bool> filter)
        {
            var newPath = GetClosestOfTypeWithPath(filter);
            if (!newPath.HasValue)
            {
                return false;
            }
            currentPath = newPath.Value;
            return true;
        }

        /// <summary>
        /// Finds a member matching filter which can be navigated to. returns an object which can be used to navigate
        ///     to the indicated target
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>null if there is no member. otherwise a navigationPath with the target member and the path to it</returns>
        public NavigationPath? GetClosestOfTypeWithPath(Func<TileMapMember, bool> filter)
        {
            var paths = AllPossiblePaths(filter);

            var minDist = float.MaxValue;
            IList<ICoordinate> bestPath = new ICoordinate[0];
            TileMapMember bestMemeber = null;

            foreach (var (path, target) in paths)
            {
                if (path.Count < minDist)
                {
                    bestPath = path;
                    minDist = bestPath.Count;
                    bestMemeber = target;
                }
            }

            if (bestMemeber == null)
            {
                return null;
            }
            return new NavigationPath
            {
                coordinatePath = bestPath,
                targetMember = bestMemeber
            };
        }

        private IEnumerable<(List<ICoordinate>, TileMapMember)> AllPossiblePaths(Func<TileMapMember, bool> filter)
        {
            var possibleSelections = currentRegion.universalContentTracker.allMembers
                .Where(filter);
            return possibleSelections.Select(member =>
                (PathfinderUtils.PathBetween(
                        coordinatePosition,
                        member.CoordinatePosition,
                        currentRegion,
                        (coord, properties) => currentRegion.universalContentTracker.IsPassableTypeUnsafe(coord))?.ToList(),
                    member)
                )
                .Where(x => x.Item1 != null);
        }

        public bool AreAnyOfTypeReachable(Func<TileMapMember, bool> filter)
        {
            return AllPossiblePaths(filter).Any();
        }
        private (List<ICoordinate>, TileMapMember) GetPathIfExists(TileMapMember member)
        {
            var path = PathfinderUtils.PathBetween(
                        coordinatePosition,
                        member.CoordinatePosition,
                        currentRegion,
                        (coord, properties) => currentRegion.universalContentTracker.IsPassableTypeUnsafe(coord))?.ToList();
            return (path,
                    member);
        }
        public bool IsReachable(TileMapMember member)
        {
            var foundPath = GetPathIfExists(member);
            return foundPath.Item1 != null;
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
}