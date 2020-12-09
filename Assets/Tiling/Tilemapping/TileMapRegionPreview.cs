using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tiling.Tilemapping
{
    /// <summary>
    /// configured and set up inside a prefab. doesn't need to pull data from configuration data, because it will never save
    /// </summary>
    public class TileMapRegionPreview : TileMapRegionRenderer
    {
        public TileMapRegionData ownRegionData;

        public Button ConfirmButton;
        public Button CancelButton;
        public GameObject confirmUIParent;
        protected override void Awake()
        {
            base.Awake();
        }

        public override void InitializeForTopologyBake(
            TileMapConfigurationData tileConfiguration,
            UniversalCoordinateSystemMembers members)
        {
        }

        public override void BakeTopology(
            TileMapRegionData data,
            IEnumerable<TileMapRegionRenderer> otherRegionsToAvoid)
        {
            var points = data.baseRange.BoundingPolygon();
            var triangles = data.baseRange.BoundingPolyTriangles();

            var newMesh = new Mesh();
            newMesh.SetVertices(points
                .Select(point => data.coordinateTransform.MultiplyPoint3x4(point))
                .ToArray());
            newMesh.SetTriangles(triangles, 0);
            newMesh.SetUVs(0, points.ToArray());

            var meshComponent = GetComponent<MeshFilter>();
            meshComponent.mesh = newMesh;
        }
    }
}
