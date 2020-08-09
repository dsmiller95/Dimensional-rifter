using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public abstract class TileMapRegionNoCoordinateType : MonoBehaviour
    {
        public string defaultTile;
        public float sideLength = 1;

        public abstract PolygonCollider2D SetupBoundingCollider();
        public abstract void BakeMeshAvoidingColliders(IEnumerable<PolygonCollider2D> collidersToAvoid);
        /// <summary>
        /// Update the mesh based on a given list of colliders. will hide the tiles which overlap with any of the colliders
        ///     this is faster than baking the mesh, so should be used to update while things are moving. Bake the mesh when things 
        ///     stop moving
        /// Will only hide the tiles which are colliding with a collider in collidersToAvoid, if some were hidden before then they will get shown again
        ///     if they no longer intersect
        /// </summary>
        /// <param name="collidersToAvoid"></param>
        public abstract void UpdateMeshTilesBasedOnColliders(IEnumerable<PolygonCollider2D> collidersToAvoid);
    }

    [RequireComponent(typeof(PolygonCollider2D))]
    public abstract class TileMapRegion<T> : TileMapRegionNoCoordinateType where T : ICoordinate
    {
        protected GenericTileMapContainer<T> tileMapContainer;
        protected ITileMapSystem<T> tileMapSystem;

        protected PolygonCollider2D BoundingBoxCollider;
        protected PolygonCollider2D IndividualCellCollider;

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
        }

        public abstract ICoordinateRange<T> CoordinateRange { get; }
        public abstract ICoordinateSystem<T> UnscaledCoordinateSystem { get; }
        public abstract ICoordinateSystem<T> WorldSpaceCoordinateSystem { get; }
        public override void BakeMeshAvoidingColliders(IEnumerable<PolygonCollider2D> collidersToAvoid = null)
        {
            var colliderList = collidersToAvoid.ToList();
            var setupMesh = tileMapContainer.BakeTilemapMesh(
                CoordinateRange,
                defaultTile,
                UnscaledCoordinateSystem,
                (coord, position) =>
                    !GetCollidesWith(coord, colliderList));

            var meshHolder = GetComponent<MeshFilter>();
            meshHolder.mesh = setupMesh;
        }

        public override void UpdateMeshTilesBasedOnColliders(IEnumerable<PolygonCollider2D> collidersToAvoid)
        {
            var colliders = collidersToAvoid.ToList();
            foreach (var loadedCoordinate in tileMapContainer.GetBakedTiles())
            {
                var collides = this.GetCollidesWith(loadedCoordinate, colliders);
                tileMapContainer.SetTileEnabled(loadedCoordinate, !collides);
            }
        }

        private bool GetCollidesWith(T coord, IList<PolygonCollider2D> otherBounds)
        {
            // TODO: performance
            // could use a bounding-box overlap check to pre-check which bounds we might collide with, before comparing all the points

            var individualTileVertexesWorldSpace = GetTileBoundingVertsWorldSpace(coord).ToArray();

            var pointCollides = otherBounds
                .Any(collide =>
                    individualTileVertexesWorldSpace
                        .Any(point => collide.OverlapPoint(point))
                 );

            if (!pointCollides)
            {
                //if the individual points are inside the big collider list
                // then check to see if any of the big collider's points are inside the individual tile

                var colliderPointsWorldSpace = otherBounds.SelectMany(x => x.GetPath(0).Select(point => x.transform.TransformPoint(point)));

                var individualTileCollider = SetupIndividualCollider(GetTileBoundingVertsLocalSpace(coord));

                pointCollides = colliderPointsWorldSpace.Any(point => individualTileCollider.OverlapPoint(point));
            }
            return pointCollides;
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
