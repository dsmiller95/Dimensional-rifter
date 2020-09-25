using Assets.Libraries.Utilities;
using Assets.Scripts.Utilities;
using Assets.Tiling.Tilemapping.MeshEdit;
using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    /// <summary>
    /// Data set up in map-gen; or in the inspector. Data that should be Saved and loaded
    /// </summary>
    [Serializable]
    public class TileMapRegionData
    {
        public Matrix4x4 coordinateTransform;
        public short planeIDIndex;
        public IUniversalCoordinateRange baseRange;
    }
    [Serializable]
    public class TileRegionSaveObject
    {
        public SerializableMatrix4x4 matrixSerialized;
        //public Matrix4x4 planeTransformation;
        public IUniversalCoordinateRange range;
    }

    public class TileMapRegionRuntimeData
    {
        public HashSet<UniversalCoordinate> disabledCoordinates;
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class TileMapRegionBehavior : MonoBehaviour
    {
        protected PolygonCollider2D BoundingBoxCollider;
        protected PolygonCollider2D IndividualCellCollider;
        public TileMapMeshBuilderBETTER meshBuilder;

        public TileMapRegionRuntimeData runtimeData;
        public TheReelBigCombinationTileMapManager BigManager => GetComponentInParent<TheReelBigCombinationTileMapManager>();

        protected virtual void Awake()
        {
            var polygons = GetComponents<PolygonCollider2D>();
            if (polygons.Length != 2)
            {
                throw new Exception("not enough polygon colliders to use");
            }
            BoundingBoxCollider = polygons[0];
            IndividualCellCollider = polygons[1];

            runtimeData = new TileMapRegionRuntimeData();
        }

        public UniversalCoordinate? GetCoordinateFromRealPositionIffValid(
            Vector2 realPositionInPlane,
            TileMapRegionData data)
        {
            Vector2 pointInPlane = data.coordinateTransform.inverse.MultiplyPoint3x4(realPositionInPlane);
            var coord = UniversalCoordinate.FromPositionInPlane(pointInPlane, data.baseRange.coordinateType, data.planeIDIndex);
            if (IsValidInThisPlane(coord, data))
            {
                return coord;
            }
            return null;
        }

        public bool IsValidInThisPlane(UniversalCoordinate coordinate, TileMapRegionData data)
        {
            if (!data.baseRange.ContainsCoordinate(coordinate))
            {
                return false;
            }
            return !runtimeData.disabledCoordinates.Contains(coordinate);
        }

        public void InitializeMeshBuilder(
            TileMapConfigurationData tileConfiguration,
            UniversalCoordinateSystemMembers members)
        {

            meshBuilder = new TileMapMeshBuilderBETTER(tileConfiguration.tileSet, members);
            var renderer = GetComponent<MeshRenderer>();
            renderer.material.mainTexture = tileConfiguration.tileTexture;
            meshBuilder.SetupTilesForGivenTexture(tileConfiguration.tileTexture);
        }

        public PolygonCollider2D SetupBoundingCollider(TileMapRegionData data)
        {
            var path = data.baseRange.BoundingPolygon()
                .Select(point => (Vector2)data.coordinateTransform.MultiplyPoint3x4(point));
            BoundingBoxCollider.SetPath(0, path.ToArray());
            return BoundingBoxCollider;
        }

        public PolygonCollider2D SetupIndividualCollider(Vector2[] vertexes)
        {
            IndividualCellCollider.SetPath(0, vertexes.ToArray());
            return IndividualCellCollider;
        }

        public void BakeTopologyAvoidingOthers(
            TileMapRegionData data,
            IEnumerable<TileMapRegionBehavior> otherRegionsToAvoid)
        {
            var colliderList = otherRegionsToAvoid.Select(x => x.BoundingBoxCollider).ToArray();
            var colliderFlagSpace = colliderList.Select(x => false).ToArray();
            runtimeData.disabledCoordinates = new HashSet<UniversalCoordinate>();
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
                        runtimeData.disabledCoordinates.Add(coord);
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


        public void AddConnectivityAndMemberData(
            TileMapRegionData data,
            ConnectivityGraphBuilder connectivityGraphBuilder)
        {
            int beginningIndex = connectivityGraphBuilder.CurrentNodeCount();
            var allMembers = BigManager.everyMember;

            int currentIndex = beginningIndex;
            var coordinatesInMap = data.baseRange.GetUniversalCoordinates(data.planeIDIndex)
                .Where(coord => !runtimeData.disabledCoordinates.Contains(coord));
            var coordinateToIndexMap = coordinatesInMap.ToDictionary(coord => coord, coord =>
            {
                return currentIndex++;
            });
            foreach (var coordinate in coordinatesInMap)
            {
                var neighborIDs = coordinate.Neighbors()
                    .TryPullFromDictionary(coordinateToIndexMap);

                var newNode = new ConnectivityGraphNodeBuilder(neighborIDs);

                newNode.isPassable = allMembers.IsPassable(coordinate);
                newNode.membersHere = allMembers.GetMembersOnTile(coordinate);

                connectivityGraphBuilder.AddNextNode(newNode);
            }
        }
    }
}
