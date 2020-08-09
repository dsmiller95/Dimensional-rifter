using Assets.Tiling.SquareCoords;
using Extensions;
using System.Collections.Generic;
using System.Linq;
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
        public override ICoordinateSystem<SquareCoordinate> CoordinateSystem => coordSystem.coordinateSystem;

        public string editTile;

        protected override void Awake()
        {
            base.Awake();
            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            tileMapSystem = new SquareTileMapSystem();
            tileMapContainer = new GenericTileMapContainer<SquareCoordinate>(tileSet, tileMapSystem);

            tileMapContainer.SetupTilesOnGivenTexture(
                mainTex);


            tiles = new Dictionary<SquareCoordinate, string>();
            tiles[new SquareCoordinate(0, 0)] = "ground";
            tiles[new SquareCoordinate(1, 1)] = "ground";
            tiles[new SquareCoordinate(2, 2)] = "ground";
            tiles[new SquareCoordinate(0, 1)] = "water";
            tiles[new SquareCoordinate(0, 2)] = "water";
            tiles[new SquareCoordinate(0, -1)] = "water";
            tiles[new SquareCoordinate(0, -2)] = "water";
        }

        public override void Start()
        {
            base.Start();
            var manager = GetComponentInParent<CombinationTileMapManager>();
            if (manager == null || !manager.isActiveAndEnabled)
            {
                SetupMyMesh();
            }
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var point = Utilities.GetMousePos2D();
                var coords = coordSystem.coordinateSystem.FromRealPosition(point);
                tileMapContainer.SetTile(coords, editTile);
            }
        }
    }
}
