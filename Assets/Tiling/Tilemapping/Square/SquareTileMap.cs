using Assets.Tiling.SquareCoords;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Square
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class SquareTileMap : MonoBehaviour
    {
        public TileSet<SquareCoordinate> tileSet;

        public SquareCoordinateSystemBehavior coordSystem;
        public SquareCoordinateRange coordRange;

        public Dictionary<SquareCoordinate, string> tiles;
        public string defaultTile;
        public string editTile;

        private GenericTileMapContainer<SquareCoordinate> tileMapContainer;

        private void Awake()
        {

            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            var tileMapSystem = new SquareTileMapSystem();
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

        public void Start()
        {
            var setupMesh = tileMapContainer.SetupTilemapMesh(
                coordRange,
                tiles,
                defaultTile,
                coordSystem.coordinateSystem);

            var meshHolder = GetComponent<MeshFilter>();
            meshHolder.mesh = setupMesh;
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
