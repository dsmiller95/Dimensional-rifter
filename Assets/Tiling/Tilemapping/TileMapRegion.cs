using Assets.Tiling.Tilemapping.MeshEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class TileMapRegion : TileMapRegionRenderer
    {
        public TileMapMeshBuilder meshBuilder;
        public TileMapPreviewsByCoordinateRangeType previewIndex;
        protected PolygonCollider2D IndividualCellCollider;

        protected override void Awake()
        {
            base.Awake();
            var polygons = GetComponents<PolygonCollider2D>();
            if (polygons.Length < 2)
            {
                throw new Exception("not enough polygon colliders to use");
            }
            IndividualCellCollider = polygons[1];
        }

        public PolygonCollider2D SetupIndividualCollider(Vector2[] vertexes)
        {
            IndividualCellCollider.SetPath(0, vertexes.ToArray());
            return IndividualCellCollider;
        }

        public override void InitializeForTopologyBake(
            TileMapConfigurationData tileConfiguration,
            UniversalCoordinateSystemMembers members)
        {
            meshBuilder = new TileMapMeshBuilder(tileConfiguration.tileSet, members);
            var renderer = GetComponent<MeshRenderer>();
            renderer.material = tileConfiguration.tileMaterial;
            meshBuilder.SetupTilesForGivenTexture(renderer.material.mainTexture);
        }
        public void SetNoPreviews(TileMapRegionData data)
        {
            foreach (var noLongerHidden in data.runtimeData.previewFadeoutCoordiantes)
            {
                meshBuilder.EnableCoordinate(noLongerHidden);
            }
            data.runtimeData.previewFadeoutCoordiantes.Clear();
        }
        public void SetPreviewOnCollidesWith(
            TileMapRegionData data,
            IEnumerable<TileMapRegionPreview> previewRegions
            )
        {
            var oldFadeoutCoordinates = data.runtimeData.previewFadeoutCoordiantes;
            var newFadeoutCoordinates = new HashSet<UniversalCoordinate>();

            var boundingColliderAsList = previewRegions
                .Select(x => x.RangeBoundsCollider).ToArray();// new[] { boundingCollider };
            var colliderFlagSpace = boundingColliderAsList.Select(x => false).ToArray();
            foreach (var coordinate in data.baseRange.GetUniversalCoordinates())
            {
                if (GetCollidesWith(data.coordinateTransform, coordinate, boundingColliderAsList, colliderFlagSpace))
                {
                    newFadeoutCoordinates.Add(coordinate);
                }
            }
            // TODO: utility to change the material instead of moving the geometry around
            foreach (var noLongerHidden in oldFadeoutCoordinates.Except(newFadeoutCoordinates))
            {
                meshBuilder.EnableCoordinate(noLongerHidden);
            }
            foreach (var newlyHidden in newFadeoutCoordinates.Except(oldFadeoutCoordinates))
            {
                meshBuilder.DisableCoordiante(newlyHidden);
            }
            data.runtimeData.previewFadeoutCoordiantes = newFadeoutCoordinates;
        }

        public override void BakeTopology(
            TileMapRegionData data,
            IEnumerable<TileMapRegionRenderer> otherRegionsToAvoid)
        {
            var colliderList = otherRegionsToAvoid
                .Where(x => x is TileMapRegion)
                .Select(x => x.RangeBoundsCollider).ToArray();
            var colliderFlagSpace = colliderList.Select(x => false).ToArray();
            data.runtimeData.disabledCoordinates = new HashSet<UniversalCoordinate>();
            data.runtimeData.previewFadeoutCoordiantes = new HashSet<UniversalCoordinate>();
            var setupMesh = meshBuilder.BakeTilemapMesh(
                data.baseRange,
                (coord, position) =>
                {
                    if (GetCollidesWith(
                        data.coordinateTransform,
                        coord,
                        colliderList,
                        colliderFlagSpace))
                    {
                        data.runtimeData.disabledCoordinates.Add(coord);
                        return false;
                    }
                    return true;
                });

            Debug.Log("assigning mesh");
            var meshHolder = GetComponent<MeshFilter>();
            meshHolder.mesh = setupMesh;
        }
        private bool GetCollidesWith(
            Matrix4x4 planeToWorldSpaceTransform,
            UniversalCoordinate coord,
            PolygonCollider2D[] otherBounds,
            bool[] anyPossibleCollisionFlags)
        {
            var colliderBounds = coord.GetRawBounds(1, planeToWorldSpaceTransform);

            var anyColliders = false;
            for (var i = 0; i < otherBounds.Length; i++)
            {
                anyPossibleCollisionFlags[i] = otherBounds[i].bounds.Intersects(colliderBounds);
                anyColliders |= anyPossibleCollisionFlags[i];
            }
            if (!anyColliders)
            {
                return false;
            }

            var tileVertexes = coord.GetVertexesAround().ToArray();
            var tileVertexesWorldSpace = tileVertexes
                .Select(vert => (Vector2)planeToWorldSpaceTransform.MultiplyPoint3x4(vert))
                .ToArray();

            // var individualTileVertexesWorldSpace = GetTileBoundingVertsWorldSpace(coord).ToArray();

            for (var i = 0; i < otherBounds.Length; i++)
            {
                if (anyPossibleCollisionFlags[i])
                {
                    var collidesWithThisCollider = tileVertexesWorldSpace
                        .Any(point => otherBounds[i].OverlapPoint(point));
                    if (collidesWithThisCollider)
                    {
                        return true;
                    }
                }
            }

            var individualTileCollider = SetupIndividualCollider(tileVertexesWorldSpace);

            for (var i = 0; i < otherBounds.Length; i++)
            {
                if (anyPossibleCollisionFlags[i])
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
    }
}
