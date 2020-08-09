using Assets.Tiling.SquareCoords;
using Extensions;
using System.Collections.Generic;
using System.Linq;
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

        public void OnDrawGizmos()
        {
            var path = coordRange.BoundingPolygon(coordSystem.coordinateSystem, 1).ToList();
            Gizmos.color = Color.blue;
            float size = .5f;
            foreach (var pair in path.RollingWindow(2))
            {
                Gizmos.DrawLine(pair[0], pair[1]);
                Gizmos.DrawSphere(pair[0], size);
                size += .3f;
            }
            Gizmos.DrawLine(path.First(), path.Last());
            Gizmos.DrawSphere(path.Last(), size);
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
