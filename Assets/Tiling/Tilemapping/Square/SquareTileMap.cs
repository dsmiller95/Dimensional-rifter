using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.WorldObjects;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Square
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class SquareTileMap : TileMapRegion<SquareCoordinate>
    {
        public TileSet<SquareCoordinate> tileSet;

        public SquareCoordinateSystemBehavior coordSystem;
        public SquareCoordinateRange coordRangeDefaultForInspector;

        private ICoordinateRange<SquareCoordinate> _coordRange;

        public override ICoordinateRange<SquareCoordinate> CoordinateRange
        {
            get
            {
                if (_coordRange == default)
                {
                    _coordRange = coordRangeDefaultForInspector;
                }
                return _coordRange;
            }
            protected set => _coordRange = value;
        }
        public override ICoordinateSystem<SquareCoordinate> UnscaledCoordinateSystem => coordSystem.BaseCoordinateSystem;
        public override ICoordinateSystem<SquareCoordinate> WorldSpaceCoordinateSystem => coordSystem.TransformedCoordinateSystem;

        public TileTypeInfo editTile;

        protected override void Awake()
        {
            base.Awake();
            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            tileMapSystem = new SquareNonRotatedTileMapSystem();
            tileMapMeshRenderer = new TileMapMeshBuilder<SquareCoordinate>(tileSet, tileMapSystem, this.contentTracker);

            tileMapMeshRenderer.SetupTilesOnGivenTexture(
                mainTex);

            contentTracker.SetTile(new SquareCoordinate(0, 0), new TileTypeInfo("ground","NO_BORDERS"));
            contentTracker.SetTile(new SquareCoordinate(1, 1), new TileTypeInfo("ground","NO_BORDERS"));
            contentTracker.SetTile(new SquareCoordinate(2, 2), new TileTypeInfo("ground","NO_BORDERS"));
            contentTracker.SetTile(new SquareCoordinate(0, -1), new TileTypeInfo("ground","EDGESX"));
            contentTracker.SetTile(new SquareCoordinate(1, -1), new TileTypeInfo("ground","EDGE_T"));
            contentTracker.SetTile(new SquareCoordinate(2, -2), new TileTypeInfo("ground","ALLEXCEPT_B"));
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
                var coords = coordSystem.TransformedCoordinateSystem.FromRealPosition(point);
                contentTracker.SetTile(coords, editTile);
            }
        }
    }
}
