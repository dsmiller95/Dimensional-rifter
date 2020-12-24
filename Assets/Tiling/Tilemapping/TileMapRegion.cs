using Assets.Tiling.Tilemapping.MeshEdit;
using Assets.WorldObjects.Members.Buildings.DOTS;
using Assets.WorldObjects.Members.Buildings.DOTS.Anchor;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{

    [RequireComponent(typeof(PolygonCollider2D))]
    public class TileMapRegion : TileMapRegionRenderer
    {
        public TileMapMeshBuilder meshBuilder;
        protected PolygonCollider2D IndividualCellCollider;

        private EntityQuery anchorEntities;

        private bool isBuilt;

        protected override void Awake()
        {
            base.Awake();
            var polygons = GetComponents<PolygonCollider2D>();
            if (polygons.Length < 2)
            {
                throw new Exception("not enough polygon colliders to use");
            }
            IndividualCellCollider = polygons[1];
            anchorEntities = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(TilemapAnchorComponent) },
                    Options = EntityQueryOptions.IncludeDisabled
                });
        }

        private void Start()
        {
            isBuilt = MyOwnData.baseRange.CoordinatePlaneID == 0;
            CombinationTileMapManager.instance.OnRegionRenderParametersChanged();
        }

        private void Update()
        {
            var lastIsBuilt = isBuilt;
            isBuilt = GetIsBuilt();
            if (lastIsBuilt != isBuilt)
            {
                Debug.Log("Isbuild changed to " + isBuilt);
                CombinationTileMapManager.instance.OnRegionRenderParametersChanged();
            }
        }
        protected void OnDestroy()
        {
            MyOwnData.runtimeData.Dispose();
        }

        private bool GetIsBuilt()
        {
            if (MyOwnData.baseRange.CoordinatePlaneID == 0)
            {
                return true;
            }
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var anchors = anchorEntities.ToEntityArray(Unity.Collections.Allocator.Temp);
            MyOwnData.anchorEntities = anchors
                .Where(anc => entityManager.GetComponentData<TilemapAnchorComponent>(anc).destinationCoordinate.CoordinatePlaneID == MyOwnData.baseRange.CoordinatePlaneID)
                .ToList();

            var requiredAnchors = MyOwnData.baseRange.BoundingVertexCount();
            if (MyOwnData.anchorEntities.Count != requiredAnchors)
            {
                foreach (var anchor in MyOwnData.anchorEntities)
                {
                    var buildData = entityManager.GetComponentData<BuildingParentComponent>(anchor);
                    if (buildData.isBuilt)
                    {
                        continue;
                    }
                    entityManager.DestroyEntity(buildData.buildingEntity);
                    entityManager.DestroyEntity(anchor);
                }
                CombinationTileMapManager.instance.DestroyRegion(MyOwnData.baseRange.CoordinatePlaneID);
                return false;
            }
            foreach (var anchor in MyOwnData.anchorEntities)
            {
                var buildData = entityManager.GetComponentData<BuildingParentComponent>(anchor);
                if (!buildData.isBuilt)
                {
                    return false;
                }
            }
            return true;
        }

        public PolygonCollider2D SetupIndividualTileCollider(Vector2[] vertexes)
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
            renderer.material = isBuilt ? tileConfiguration.tileMaterial : tileConfiguration.tilePreviewMaterial;
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
            if (!isBuilt)
            {
                SetNoPreviews(data);
                return;
            }
            var oldFadeoutCoordinates = data.runtimeData.previewFadeoutCoordiantes;
            var newFadeoutCoordinates = new NativeHashSet<UniversalCoordinate>(oldFadeoutCoordinates.Count(), Allocator.Persistent);

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
            foreach (var oldFaded in oldFadeoutCoordinates)
            {
                if (!newFadeoutCoordinates.Contains(oldFaded))
                {
                    meshBuilder.EnableCoordinate(oldFaded);
                }
            }
            foreach (var newFaded in newFadeoutCoordinates)
            {
                if (!oldFadeoutCoordinates.Contains(newFaded))
                {
                    meshBuilder.DisableCoordiante(newFaded);
                }
            }
            data.runtimeData.previewFadeoutCoordiantes = newFadeoutCoordinates;
            oldFadeoutCoordinates.Dispose();
        }

        public override void BakeTopology(
            TileMapRegionData data,
            IEnumerable<TileMapRegionRenderer> otherRegionsToAvoid)
        {
            var colliderList = otherRegionsToAvoid
                .Where(x => x is TileMapRegion)
                .Select(x => x.RangeBoundsCollider).ToArray();
            var colliderFlagSpace = colliderList.Select(x => false).ToArray();
            data.runtimeData.disabledCoordinates.Clear();
            data.runtimeData.previewFadeoutCoordiantes.Clear();
            var setupMesh = meshBuilder.BakeTilemapMesh(
                data.baseRange,
                data.coordinateTransform,
                coord =>
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

            var individualTileCollider = SetupIndividualTileCollider(tileVertexesWorldSpace);

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
