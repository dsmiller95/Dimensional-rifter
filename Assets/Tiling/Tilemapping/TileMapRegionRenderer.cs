using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    /// <summary>
    /// Data set up in map-gen; or in the inspector. Data that should be Saved and loaded
    /// </summary>
    public class TileMapRegionData
    {
        public Matrix4x4 coordinateTransform;
        /// <summary>
        /// planeID of 0 is assumed to be the root region
        /// </summary>
        public UniversalCoordinateRange baseRange;
        public bool preview;
        public List<Entity> anchorEntities;

        public TileMapRegionRuntimeData runtimeData = new TileMapRegionRuntimeData();
        public UniversalCoordinate? GetCoordinateFromRealPositionIffValid(Vector2 realPositionInPlane)
        {
            var coord = GetCoordinateFromRealPosition(realPositionInPlane);
            if (IsValidInThisPlane(coord))
            {
                return coord;
            }
            return null;
        }
        public UniversalCoordinate GetCoordinateFromRealPosition(Vector2 realPositionInPlane)
        {
            Vector2 pointInPlane = coordinateTransform.inverse.MultiplyPoint3x4(realPositionInPlane);
            return UniversalCoordinate.FromPositionInPlane(pointInPlane, baseRange.CoordinateType, baseRange.CoordinatePlaneID);
        }

        public bool IsValidInThisPlane(UniversalCoordinate coordinate)
        {
            if (!baseRange.ContainsCoordinate(coordinate))
            {
                return false;
            }
            return !runtimeData.disabledCoordinates.Contains(coordinate);
        }

        public IEnumerable<Vector2> BoundingPoints()
        {
            return baseRange.BoundingPolygon()
                .Select(point => (Vector2)coordinateTransform.MultiplyPoint3x4(point));
        }

        public UniversalCoordinate GetClosestValidCoordinate(Vector2 realPosition)
        {
            Vector2 pointInPlane = coordinateTransform.inverse.MultiplyPoint3x4(realPosition);
            var origin = UniversalCoordinate.FromPositionInPlane(pointInPlane, baseRange.CoordinateType, baseRange.CoordinatePlaneID);

            if (!baseRange.ContainsCoordinate(origin))
            {
                return default;
            }

            var checkedNeighbors = new HashSet<UniversalCoordinate>();
            var fringe = new NativeQueue<UniversalCoordinate>(Allocator.Temp);
            fringe.Enqueue(origin);

            var minDistance = float.MaxValue;
            UniversalCoordinate minDistCoordinate = default;

            while (!fringe.IsEmpty() && checkedNeighbors.Count() < 10)
            {
                var nextCoordinate = fringe.Dequeue();
                if (checkedNeighbors.Contains(nextCoordinate))
                {
                    continue;
                }
                checkedNeighbors.Add(nextCoordinate);
                foreach (var neighborCoordinate in nextCoordinate.Neighbors())
                {
                    if (checkedNeighbors.Contains(neighborCoordinate))
                    {
                        continue;
                    }
                    fringe.Enqueue(neighborCoordinate);
                }

                if (runtimeData.disabledCoordinates.Contains(nextCoordinate) || runtimeData.previewFadeoutCoordiantes.Contains(nextCoordinate))
                {
                    continue;
                }
                var coordPos = nextCoordinate.ToPositionInPlane();
                var distance = Vector2.Distance(coordPos, realPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistCoordinate = nextCoordinate;
                }
            }

            fringe.Dispose();

            return minDistCoordinate;
        }

        public TileRegionSaveObject Serialize()
        {
            if (preview)
            {
                return null;
            }
            return new TileRegionSaveObject
            {
                matrixSerialized = new SerializableMatrix4x4(coordinateTransform),
                range = baseRange
            };
        }

        public static TileMapRegionData Deserialize(TileRegionSaveObject serialized)
        {
            return new TileMapRegionData
            {
                coordinateTransform = GetSerializedTransform(serialized),
                baseRange = serialized.range
            };
        }

        private static Matrix4x4 GetSerializedTransform(TileRegionSaveObject regionPlane)
        {
            return regionPlane?.matrixSerialized?.GetMatrix() ?? Matrix4x4.identity;
        }
    }
    [Serializable]
    public class TileRegionSaveObject
    {
        public SerializableMatrix4x4 matrixSerialized;
        public UniversalCoordinateRange range;
    }

    public class TileMapRegionRuntimeData : IDisposable
    {
        public NativeHashSet<UniversalCoordinate> disabledCoordinates = new NativeHashSet<UniversalCoordinate>(1, Allocator.Persistent);
        public NativeHashSet<UniversalCoordinate> previewFadeoutCoordiantes = new NativeHashSet<UniversalCoordinate>(1, Allocator.Persistent);
        public ReadWriteJobHandleProtector readWriteLock = new ReadWriteJobHandleProtector();

        public void Dispose()
        {
            readWriteLock.Dispose();
            disabledCoordinates.Dispose();
            previewFadeoutCoordiantes.Dispose();
        }
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public abstract class TileMapRegionRenderer : MonoBehaviour
    {
        public PolygonCollider2D RangeBoundsCollider;
        public TileMapRegionData MyOwnData;
        public CombinationTileMapManager BigManager => GetComponentInParent<CombinationTileMapManager>();

        protected virtual void Awake()
        {
            var polygons = GetComponents<PolygonCollider2D>();
            if (polygons.Length < 1)
            {
                throw new Exception("not enough polygon colliders to use");
            }
            RangeBoundsCollider = polygons[0];
        }

        public PolygonCollider2D SetupBoundingCollider(TileMapRegionData data)
        {
            RangeBoundsCollider.SetPath(0, data.BoundingPoints().ToArray());
            return RangeBoundsCollider;
        }

        public abstract void InitializeForTopologyBake(
            TileMapConfigurationData tileConfiguration,
            UniversalCoordinateSystemMembers members);


        public abstract void BakeTopology(
            TileMapRegionData data,
            IEnumerable<TileMapRegionRenderer> otherRegionsToAvoid);
    }
}


