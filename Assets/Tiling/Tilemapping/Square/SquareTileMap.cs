using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping.TileConfiguration;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Square
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class SquareTileMap : TileMapRegion<SquareCoordinate>
    {
        public TileSet<SquareCoordinate> tileSet;

        public SquareCoordinateSystemBehavior coordSystem;
        public SquareCoordinateRange coordRange;

        public override ICoordinateRange<SquareCoordinate> CoordinateRange => coordRange;
        public override ICoordinateSystem<SquareCoordinate> UnscaledCoordinateSystem => coordSystem.BaseCoordinateSystem;
        public override ICoordinateSystem<SquareCoordinate> WorldSpaceCoordinateSystem => coordSystem.coordinateSystem;

        public string editTile;

        protected override void Awake()
        {
            base.Awake();
            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            tileMapSystem = new SquareNonRotatedTileMapSystem();
            tileMapMeshRenderer = new TileMapMeshBuilder<SquareCoordinate>(tileSet, tileMapSystem, this.contentTracker);

            tileMapMeshRenderer.SetupTilesOnGivenTexture(
                mainTex);

            contentTracker.SetTile(new SquareCoordinate(0, 0), "groundNO_BORDERS");
            contentTracker.SetTile(new SquareCoordinate(1, 1), "groundNO_BORDERS");
            contentTracker.SetTile(new SquareCoordinate(2, 2), "groundNO_BORDERS");
            contentTracker.SetTile(new SquareCoordinate(0, -1), "groundEDGESX");
            contentTracker.SetTile(new SquareCoordinate(1, -1), "groundEDGE_T");
            contentTracker.SetTile(new SquareCoordinate(2, -2), "groundALLEXCEPT_B");
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

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var point = MyUtilities.GetMousePos2D();
                var coords = coordSystem.coordinateSystem.FromRealPosition(point);
                contentTracker.SetTile(coords, editTile);
            }
        }
    }
}
