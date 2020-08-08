using Assets.MapGen;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping.Triangle;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Square
{
    [Serializable]
    public struct SquareTileMapTile
    {
        public SquareCoordinate coords0;
        public string ID;
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class SquareTileMap : MonoBehaviour
    {
        public SquareTileMapTile[] tileTypes;
        public float sideLength;
        public float tilePadding;


        private Dictionary<string, MultiVertTileConfig> tileTypesDictionary;

        public SquareCoordinateSystemBehavior coordSystem;
        public SquareCoordinateRange coordRange;

        public Dictionary<SquareCoordinate, string> tiles;
        public string defaultTile;
        public string editTile;

        private GenericTileMapContainer<SquareCoordinate> tileMapContainer;
        private ITileMapSystem<SquareCoordinate> tileMapSystem;

        private void Awake()
        {

            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;

            tileMapSystem = new SquareTileMapSystem();
            tileMapContainer = new GenericTileMapContainer<SquareCoordinate>(new TileTextureData
            {
                sideLength = sideLength,
                padding = tilePadding
            }, tileMapSystem);

            var convertedUVs = tileMapContainer.ConvertToStandardUVConfig(
                tileTypes.Select(x => new TileConfig<SquareCoordinate>
                {
                    ID = x.ID,
                    tileCoordinate = x.coords0
                }),
                mainTex);
            tileTypesDictionary = convertedUVs.ToDictionary(x => x.ID);


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
            SetupTilemap();
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var point = Utilities.GetMousePos2D();
                var coords = coordSystem.coordinateSystem.FromRealPosition(point);

                if (coordinateCopyIndexes.TryGetValue(coords, out var index))
                {
                    if (tileTypesDictionary.TryGetValue(editTile, out var tileconfig))
                    {
                        meshEditor.SetUVForVertexesAtDuplicate(index, tileconfig.uvs);
                    }
                }
            }
        }

        private CopiedMeshEditor meshEditor;
        private Dictionary<SquareCoordinate, int> coordinateCopyIndexes;

        private void SetupTilemap()
        {
            Mesh sourceMesh = new Mesh();
            sourceMesh.subMeshCount = 1;
            sourceMesh.SetVertices(
                SquareCoordinateSystem.GetSquareVertsAround(new SquareCoordinate(0, 0), 1)
                .Select(x => (Vector3)x)
                .ToList());
            sourceMesh.SetColors(new Color[]
            {
                Color.white,
                Color.white,
                Color.white,
                Color.white,
            });
            sourceMesh.SetTriangles(new int[]
            {
                0, 1, 2,
                2, 3, 0
            }, 0);
            sourceMesh.SetUVs(0, new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
            });

            ICoordinateSystem<SquareCoordinate> rectCoords = coordSystem.coordinateSystem;
            coordinateCopyIndexes = new Dictionary<SquareCoordinate, int>();

            var targetMesh = new Mesh();
            var copier = new MeshCopier(sourceMesh, 1, targetMesh, 1);

            foreach (var coord in coordRange)
            {
                string tileType;
                if (!tiles.TryGetValue(coord, out tileType))
                {
                    tileType = defaultTile;
                }

                var tileConfig = tileTypesDictionary[tileType];
                var tileLocation = rectCoords.ToRealPosition(coord);

                var indexAdded = copier.NextCopy(tileLocation, UVOverride: tileConfig.uvs);
                copier.CopySubmeshTrianglesToOffsetIndex(0, 0);

                coordinateCopyIndexes[coord] = indexAdded;
            }

            meshEditor = copier.FinalizeCopy();

            var meshHolder = GetComponent<MeshFilter>();
            meshHolder.mesh = targetMesh;
        }

    }
}
