using Assets.Tiling.SquareCoords;
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
            tileMapContainer = new TileMapMeshBuilder<SquareCoordinate>(tileSet, tileMapSystem);

            tileMapContainer.SetupTilesOnGivenTexture(
                mainTex);

            tileMapContainer.SetTile(new SquareCoordinate(0, 0), "ground");
            tileMapContainer.SetTile(new SquareCoordinate(1, 1), "ground");
            tileMapContainer.SetTile(new SquareCoordinate(2, 2), "ground");
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
                tileMapContainer.SetTile(coords, editTile);
            }
        }
    }
}
