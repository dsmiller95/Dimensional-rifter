using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public abstract class TileMapRegionNoCoordinateType: MonoBehaviour
    {
        public string defaultTile;
        public float sideLength = 1;

        public abstract PolygonCollider2D SetupBoundingCollider();
        public abstract void SetupMyMesh(IEnumerable<PolygonCollider2D> collidersToAvoid = null);
    }

    [RequireComponent(typeof(PolygonCollider2D))]
    public abstract class TileMapRegion<T>: TileMapRegionNoCoordinateType where T : ICoordinate
    {
        protected GenericTileMapContainer<T> tileMapContainer;
        protected ITileMapSystem<T> tileMapSystem;
        public Dictionary<T, string> tiles;

        protected PolygonCollider2D BoundingBoxCollider;
        protected PolygonCollider2D IndividualCellCollider;

        protected virtual void Awake()
        {
            var polygons = this.GetComponents<PolygonCollider2D>();
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
        public abstract ICoordinateSystem<T> CoordinateSystem { get; }
        public override void SetupMyMesh(IEnumerable<PolygonCollider2D> collidersToAvoid = null)
        {
            try
            {
                Debug.Log("setting up mesh");
                var colliderList = collidersToAvoid.ToList();
                var setupMesh = tileMapContainer.SetupTilemapMesh(
                    CoordinateRange,
                    tiles,
                    defaultTile,
                    CoordinateSystem,
                    (coord, position) =>
                    {
                        var shouldHaveCollided = colliderList.Any(c => c.OverlapPoint(position));

                        var individualTileVertexes = tileMapSystem.GetVertexesAround(coord, sideLength, CoordinateSystem).ToArray();

                        var pointCollides = colliderList
                            .Any(collide =>
                                individualTileVertexes
                                    .Any(point => collide.OverlapPoint(point))
                             );

                        if (!pointCollides)
                        {
                            //if the individual points are inside the big collider list
                            // then check to see if any of the big collider's points are inside the individual tile
                            var individualTileCollider = SetupIndividualCollider(individualTileVertexes);
                            pointCollides = colliderList.Any(collider =>
                                collider
                                    .GetPath(0)
                                    .Any(point => individualTileCollider.OverlapPoint(point))
                                );
                        }
                        if(shouldHaveCollided && !pointCollides)
                        {
                            Debug.Log("shoulda collided, but didn't");
                        }
                        if (pointCollides)
                        {
                            Debug.Log("did collide?");
                        }
                        return !pointCollides;
                    });

                var meshHolder = GetComponent<MeshFilter>();
                meshHolder.mesh = setupMesh;
            }
            catch(Exception e)
            {
                throw;
            }
        }
        /// <summary>
        /// sets up the shape of both the bounding box collider
        /// </summary>
        /// <returns>the bounding box collider</returns>
        public override PolygonCollider2D SetupBoundingCollider()
        {
            BoundingBoxCollider.SetPath(0, CoordinateRange.BoundingPolygon(CoordinateSystem, sideLength).ToArray());
            return BoundingBoxCollider;
        }

        /// <summary>
        /// sets up the shape of both the bounding box collider
        /// </summary>
        /// <returns>the bounding box collider</returns>
        public PolygonCollider2D SetupIndividualCollider(Vector2[] vertexes)
        {
            IndividualCellCollider.SetPath(0, vertexes.ToArray());
            return IndividualCellCollider;
        }

        public void OnDrawGizmos()
        {
            var path = CoordinateRange.BoundingPolygon(CoordinateSystem, sideLength).ToList();
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
