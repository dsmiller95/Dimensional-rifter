using Assets.Tiling;
using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
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
        public IList<UniversalCoordinate> coordinatePath;
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
        protected void Start()
        {
            lastMove = Time.time;
        }
        public float movementSpeed = .5f;
        private float lastMove;

        ConnectivityEntitySystem ConnectivityEntitySystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ConnectivityEntitySystem>();

        /// <summary>
        /// Attempt to advance the member along the given path. Will return INVALID_TARGET if there is no set target, or if 
        ///     the current path is no longer valid. this could happen if the target has been destroyed, or if something impassible has been
        ///     placed along the current path
        /// </summary>
        /// <param name="path">the path to advance along</param>
        /// <returns></returns>
        public NavigationStatus AttemptAdvanceAlongPath(NavigationPath path)
        {
            // TODO: check for interruptions along path
            if (path.coordinatePath.Count <= 0)
            {
                return NavigationStatus.ARRIVED;
            }

            var timeSinceLastMove = Time.time - lastMove;
            var movementRatio = timeSinceLastMove / movementSpeed;
            if (movementRatio <= 1)
            {
                InterplateFromCurrentTo(path.coordinatePath.First(), movementRatio);
                return NavigationStatus.APPROACHING;
            }
            lastMove = Time.time;


            var nextPosition = path.coordinatePath[0];
            path.coordinatePath.RemoveAt(0);
            SetPosition(nextPosition);

            return NavigationStatus.APPROACHING;
        }

        public NavigationPath? GetRandomPathOfLength(int steps)
        {
            var connectionSystem = ConnectivityEntitySystem;
            if (!connectionSystem.HasRegionMaps)
            {
                return null;
            }

            var path = PathfinderUtils.RandomWalkOfLength(
                coordinatePosition,
                steps,
                bigManager,
                (coord, properties) => !connectionSystem.BlockedCoordinates.Contains(coord));

            return new NavigationPath
            {
                coordinatePath = path.ToList()
            };
        }

        /// <summary>
        /// Finds a member matching filter which can be navigated to. returns an object which can be used to navigate
        ///     to the indicated target
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="navigateToAdjacent">If the path should only go to an adjacent tile, without trying to overlap with the target member</param>
        /// <returns>null if there is no member. otherwise a navigationPath with the target member and the path to it</returns>
        [Obsolete("use entities")]
        public NavigationPath? GetClosestOfTypeWithPath(Func<TileMapMember, bool> filter, bool navigateToAdjacent = true)
        {
            var paths = AllPossiblePaths(filter, navigateToAdjacent);

            var minDist = float.MaxValue;
            IList<UniversalCoordinate> bestPath = new UniversalCoordinate[0];
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

        public NavigationPath? GetPathTo(UniversalCoordinate target, bool navigateToAdjacent = true)
        {
            var connectionSystem = ConnectivityEntitySystem;
            if (!connectionSystem.HasRegionMaps)
            {
                return null;
            }
            var path = PathfinderUtils.PathBetween(
                CoordinatePosition,
                target,
                bigManager,
                (coord, properties) => !connectionSystem.BlockedCoordinates.Contains(coord),
                navigateToAdjacent
            );
            if (path == null)
            {
                return null;
            }
            return new NavigationPath
            {
                coordinatePath = path.ToList()
            };
        }

        public NavigationPath? GetPathTo(TileMapMember target, bool navigateToAdjacent = true)
        {
            var path = GetPathTo(target.CoordinatePosition, navigateToAdjacent);
            if (path.HasValue)
            {
                var returnVal = path.Value;
                returnVal.targetMember = target;
                return returnVal;
            }
            return path;
        }

        [Obsolete("Use entites")]
        private IEnumerable<(List<UniversalCoordinate>, TileMapMember)> AllPossiblePaths(Func<TileMapMember, bool> filter, bool navigateToAdjacent = true)
        {
            var myRegionID = RegionBitMask;
            var myMembershipData = CoordinatePosition.CoordinateMembershipData;
            var possibleSelections = bigManager.everyMember.allMembers
                .Where(member => member.CoordinatePosition.CoordinateMembershipData == myMembershipData &&
                    (member.RegionBitMask & myRegionID) != 0)
                .Where(filter);
            return possibleSelections.Select(member =>
                {
                    var path = PathfinderUtils.PathBetween(
                            coordinatePosition,
                            member.CoordinatePosition,
                            bigManager,
                            (coord, properties) => bigManager.everyMember.IsPassable(coord),
                            navigateToAdjacent)?.ToList();
                    return (path, member);
                })
                .Where(x => x.Item1 != null);
        }

        [Obsolete("Use entities")]
        public bool IsReachable(TileMapMember member)
        {
            return (member.RegionBitMask & RegionBitMask) != 0;
        }
    }
}