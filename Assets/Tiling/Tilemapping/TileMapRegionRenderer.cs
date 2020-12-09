using Assets.Scripts.Utilities;
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
        public UniversalCoordinateRange baseRange;
        public bool preview;
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
            return UniversalCoordinate.FromPositionInPlane(pointInPlane, baseRange.CoordinateType, planeIDIndex);
        }

        public bool IsValidInThisPlane(UniversalCoordinate coordinate)
        {
            if (!baseRange.ContainsCoordinate(coordinate))
            {
                return false;
            }
            return !runtimeData.disabledCoordinates.Contains(coordinate);
        }
    }
    [Serializable]
    public class TileRegionSaveObject
    {
        public SerializableMatrix4x4 matrixSerialized;
        public UniversalCoordinateRange range;
    }

    public class TileMapRegionRuntimeData
    {
        public HashSet<UniversalCoordinate> disabledCoordinates;
        public HashSet<UniversalCoordinate> previewFadeoutCoordiantes;
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public abstract class TileMapRegionRenderer : MonoBehaviour
    {
        public PolygonCollider2D RangeBoundsCollider;

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
            var path = data.baseRange.BoundingPolygon()
                .Select(point => (Vector2)data.coordinateTransform.MultiplyPoint3x4(point));
            RangeBoundsCollider.SetPath(0, path.ToArray());
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
