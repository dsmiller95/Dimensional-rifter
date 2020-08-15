using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Triangle
{

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TriangleTileMap : TileMapRegion<TriangleCoordinate>
    {
        public TileSet<TriangleCoordinate> tileSet;

        public TriangleCoordinateSystemBehavior coordSystem;
        public TriangleTriangleCoordinateRange coordRange;

        public override ICoordinateRange<TriangleCoordinate> CoordinateRange => coordRange;
        public override ICoordinateSystem<TriangleCoordinate> UnscaledCoordinateSystem => coordSystem.BaseCoordinateSystem;
        public override ICoordinateSystem<TriangleCoordinate> WorldSpaceCoordinateSystem => coordSystem.coordinateSystem;

        public TileTypeInfo editTile;


        protected override void Awake()
        {
            base.Awake();
            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            tileMapSystem = new TriangleTileMapSystem();
            tileMapMeshRenderer = new TileMapMeshBuilder<TriangleCoordinate>(tileSet, tileMapSystem, contentTracker);

            tileMapMeshRenderer.SetupTilesOnGivenTexture(
                mainTex);

            contentTracker.SetTile(new TriangleCoordinate(0, 0, true), new TileTypeInfo("ground", ""));
            contentTracker.SetTile(new TriangleCoordinate(0, 0, false), new TileTypeInfo("ground", ""));
            contentTracker.SetTile(new TriangleCoordinate(1, 0, false), new TileTypeInfo("ground", ""));
            contentTracker.SetTile(new TriangleCoordinate(0, 1, false), new TileTypeInfo("ground", ""));
        }

        public override void Start()
        {
            base.Start();
            var manager = GetComponentInParent<CombinationTileMapManager>();
            if (manager == null || !manager.isActiveAndEnabled)
            {
                BakeTopologyAvoidingColliders();
            }
        }

        public override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(0))
            {
                var point = MyUtilities.GetMousePos2D();
                var coords = coordSystem.coordinateSystem.FromRealPosition(point);

                contentTracker.SetTile(coords, editTile);
            }
        }
    }
}
