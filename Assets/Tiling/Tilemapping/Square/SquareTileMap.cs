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


        protected override void Awake()
        {
            tileMapSystem = new SquareNonRotatedTileMapSystem();
            base.Awake();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
