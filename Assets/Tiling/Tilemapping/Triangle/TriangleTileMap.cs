using Assets.Tiling.TriangleCoords;
using Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Triangle
{

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TriangleTileMap : MonoBehaviour
    {
        public TileSet<TriangleCoordinate> tileSet;

        public TriangleCoordinateSystemBehavior coordSystem;
        public TriangleTriangleCoordinateRange coordRange;

        public Dictionary<TriangleCoordinate, string> tiles;
        public string defaultTile;
        public string editTile;

        private GenericTileMapContainer<TriangleCoordinate> tileMapContainer;

        private void Awake()
        {
            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            var tileMapSystem = new TriangleTileMapSystem();
            tileMapContainer = new GenericTileMapContainer<TriangleCoordinate>(tileSet, tileMapSystem);

            tileMapContainer.SetupTilesOnGivenTexture(
                mainTex);

            tiles = new Dictionary<TriangleCoordinate, string>();
            tiles[new TriangleCoordinate(0, 0, true)] = "ground";
            tiles[new TriangleCoordinate(0, 0, false)] = "ground";
            tiles[new TriangleCoordinate(1, 0, false)] = "ground";
            tiles[new TriangleCoordinate(0, 1, false)] = "ground";
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

                Debug.Log(coords);
                tileMapContainer.SetTile(coords, editTile);
            }
        }
    }
}
