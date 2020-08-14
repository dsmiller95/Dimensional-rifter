using Assets.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Extensions;

namespace Assets.Tiling.Tilemapping
{
    public abstract class TileMapRegionNoCoordinateType : MonoBehaviour
    {
        public string defaultTile;
        public float sideLength = 1;

        public CoordinateSystemMembersAllCoordinates universalContentTracker;

        public abstract PolygonCollider2D SetupBoundingCollider();

        /// <summary>
        /// Bake the topology for this tile map, not baking any tiles which collide with any colliders in <paramref name="collidersToAvoid"/>
        /// </summary>
        /// <param name="collidersToAvoid">Colliders used to exclude tiles from the topology</param>
        public abstract void BakeTopologyAvoidingColliders(IEnumerable<PolygonCollider2D> collidersToAvoid);
        /// <summary>
        /// Update the mesh based on a given list of colliders. will hide the tiles which overlap with any of the colliders
        ///     this is faster than baking the mesh, so should be used to update while things are moving. Bake the mesh when things 
        ///     stop moving
        /// Will only hide the tiles which are colliding with a collider in collidersToAvoid, if some were hidden before then they will get shown again
        ///     if they no longer intersect
        /// </summary>
        /// <param name="collidersToAvoid"></param>
        public abstract void UpdateMeshTilesBasedOnColliders(IEnumerable<PolygonCollider2D> collidersToAvoid);
        public abstract ICoordinateSystem UntypedCoordianteSystemWorldSpace { get; }


    }

    /// <summary>
    /// Abstract behavior responsible for owning and coordinating the layout of tiles
    ///     implementations must instantiate their own TileMapSystem and tileMapContainer in Awake(). They must also 
    ///     provide getters for the coordinate system used to layout the tileMap, these can be set in the inspector.
    /// Must also provide a coordinate range getter which will be used to determine which tiles the <see cref="TileMapMeshBuilder{T}"/>
    ///     will instantiate, as well as how this region will overlap with other tilemap regions
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [RequireComponent(typeof(PolygonCollider2D))]
    public abstract class TileMapRegion<T> : TileMapRegionNoCoordinateType where T : ICoordinate
    {
        protected TileMapMeshBuilder<T> tileMapContainer;
        protected ITileMapTileShapeStrategy<T> tileMapSystem;

        protected PolygonCollider2D BoundingBoxCollider;
        protected PolygonCollider2D IndividualCellCollider;

        public CoordinateSystemMembers<T> contentTracker => universalContentTracker as CoordinateSystemMembers<T>;

        protected virtual void Awake()
        {
            var polygons = GetComponents<PolygonCollider2D>();
            if (polygons.Length != 2)
            {
                throw new Exception("not enough polygon colliders to use");
            }
            BoundingBoxCollider = polygons[0];
            IndividualCellCollider = polygons[1];
        }

        public virtual void Start()
        {
            contentTracker.coordinateSystem = WorldSpaceCoordinateSystem;
        }

        public abstract ICoordinateRange<T> CoordinateRange { get; }
        public abstract ICoordinateSystem<T> UnscaledCoordinateSystem { get; }
        public override ICoordinateSystem UntypedCoordianteSystemWorldSpace => WorldSpaceCoordinateSystem;
        public abstract ICoordinateSystem<T> WorldSpaceCoordinateSystem { get; }

        public ISet<T> disabledCoordinates { get; private set; }

        public override void BakeTopologyAvoidingColliders(IEnumerable<PolygonCollider2D> collidersToAvoid = null)
        {
            var colliderList = collidersToAvoid?.ToArray() ?? new PolygonCollider2D[0];
            var colliderFlagSpace = colliderList.Select(x => false).ToArray();
            disabledCoordinates = new HashSet<T>();
            var setupMesh = tileMapContainer.BakeTilemapMesh(
                CoordinateRange,
                defaultTile,
                UnscaledCoordinateSystem,
                (coord, position) =>
                    {
                        if (GetCollidesWith(coord, colliderList, colliderFlagSpace))
                        {
                            disabledCoordinates.Add(coord);
                            return false;
                        }
                        return true;
                    });

            var meshHolder = GetComponent<MeshFilter>();
            meshHolder.mesh = setupMesh;
        }

        public bool IsValidCoordinate(ICoordinate coordinate)
        {
            if(coordinate is T casted)
            {
                return CoordinateRange.ContainsCoordinate(casted) && !disabledCoordinates.Contains(casted);
            }
            return false;
        }

        public override void UpdateMeshTilesBasedOnColliders(IEnumerable<PolygonCollider2D> collidersToAvoid)
        {
            var colliders = collidersToAvoid.ToArray();
            var colliderFlagSpace = colliders.Select(x => false).ToArray();
            //TODO: Performance
            // allow for some way to limit the tiles we iterate here based on the bounds of the polygon colliders.
            //  may only be relevant to rectangular coordinates
            foreach (var loadedCoordinate in tileMapContainer.GetBakedTiles())
            {
                var collides = GetCollidesWith(loadedCoordinate, colliders, colliderFlagSpace);
                tileMapContainer.SetTileEnabled(loadedCoordinate, !collides);
            }
        }


        private bool GetCollidesWith(T coord, PolygonCollider2D[] otherBounds, bool[] colliderFlagSpace)
        {
            var colliderBounds = GetTileBounds(coord);

            var anyColliders = false;
            for (var i = 0; i < otherBounds.Length; i++)
            {
                colliderFlagSpace[i] = otherBounds[i].bounds.Intersects(colliderBounds);
                anyColliders |= colliderFlagSpace[i];
            }
            if (!anyColliders)
            {
                return false;
            }

            var individualTileVertexesWorldSpace = GetTileBoundingVertsWorldSpace(coord).ToArray();

            for (var i = 0; i < otherBounds.Length; i++)
            {
                if (colliderFlagSpace[i])
                {
                    var collidesWithThisCollider = individualTileVertexesWorldSpace
                        .Any(point => otherBounds[i].OverlapPoint(point));
                    if (collidesWithThisCollider)
                    {
                        return true;
                    }
                }
            }

            var individualTileCollider = SetupIndividualCollider(GetTileBoundingVertsLocalSpace(coord));

            for (var i = 0; i < otherBounds.Length; i++)
            {
                if (colliderFlagSpace[i])
                {
                    var bigColliderPoints = otherBounds[i]
                        .GetPath(0)
                        .Select(point => otherBounds[i].transform.TransformPoint(point));
                    if (bigColliderPoints.Any(point => individualTileCollider.OverlapPoint(point)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// sets up the shape of both the bounding box collider
        /// </summary>
        /// <returns>the bounding box collider</returns>
        public override PolygonCollider2D SetupBoundingCollider()
        {
            BoundingBoxCollider.SetPath(0, CoordinateRange.BoundingPolygon(UnscaledCoordinateSystem, sideLength).ToArray());
            return BoundingBoxCollider;
        }

        /// <summary>
        /// sets up the shape of both the bounding box collider
        /// </summary>
        /// <returns>the bounding box collider</returns>
        public PolygonCollider2D SetupIndividualCollider(IEnumerable<Vector2> vertexes)
        {
            IndividualCellCollider.SetPath(0, vertexes.ToArray());
            return IndividualCellCollider;
        }

        private Bounds GetTileBounds(T coord)
        {
            return tileMapSystem.GetRawBounds(coord, sideLength, WorldSpaceCoordinateSystem);
        }

        private IEnumerable<Vector2> GetTileBoundingVertsLocalSpace(T coord)
        {
            return tileMapSystem.GetVertexesAround(coord, sideLength, UnscaledCoordinateSystem);
        }
        private IEnumerable<Vector2> GetTileBoundingVertsWorldSpace(T coord)
        {
            return GetTileBoundingVertsLocalSpace(coord).Select(x => (Vector2)transform.TransformPoint(x));
        }

        private IEnumerable<Vector3> GetBoundsWorldSpace()
        {
            return CoordinateRange.BoundingPolygon(UnscaledCoordinateSystem, sideLength).Select(x => transform.TransformPoint(x));
        }

        public void OnDrawGizmos()
        {
            var path = GetBoundsWorldSpace().ToList();
            Gizmos.color = Color.blue;
            float size = .5f;
            foreach (var pair in path.RollingWindow(2))
            {
                Gizmos.DrawLine(pair[0], pair[1]);
                Gizmos.DrawSphere(pair[0], size);
                size += .3f;
            }
            Gizmos.DrawLine(path.First(), path.Last());
            Gizmos.DrawSphere(path.Last(), size);
        }
    }
}
