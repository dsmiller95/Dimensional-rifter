﻿using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.Tiling.TriangleCoords;
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

        public string editTile;


        protected override void Awake()
        {
            base.Awake();
            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            tileMapSystem = new TriangleTileMapSystem();
            tileMapContainer = new TileMapMeshBuilder<TriangleCoordinate>(tileSet, tileMapSystem);

            tileMapContainer.SetupTilesOnGivenTexture(
                mainTex);

            tileMapContainer.SetTile(new TriangleCoordinate(0, 0, true), "ground");
            tileMapContainer.SetTile(new TriangleCoordinate(0, 0, false), "ground");
            tileMapContainer.SetTile(new TriangleCoordinate(1, 0, false), "ground");
            tileMapContainer.SetTile(new TriangleCoordinate(0, 1, false), "ground");
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
