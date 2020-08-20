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
        public TriangleCoordinateSystemBehavior coordSystem;
        public TriangleTriangleCoordinateRange coordRangeDefaultForInspector;

        private ICoordinateRange<TriangleCoordinate> _coordRange;

        public override ICoordinateRange<TriangleCoordinate> CoordinateRange
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
        public override ICoordinateSystem<TriangleCoordinate> UnscaledCoordinateSystem => coordSystem.BaseCoordinateSystem;
        public override ICoordinateSystem<TriangleCoordinate> WorldSpaceCoordinateSystem => coordSystem.TransformedCoordinateSystem;


        protected override void Awake()
        {
            tileMapSystem = new TriangleTileMapSystem();
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
