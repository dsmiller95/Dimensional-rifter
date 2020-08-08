using Assets.MapGen;
using Assets.Tiling.TriangleCoords;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Triangle
{
    [System.Serializable]
    public struct TriangleTileMapTile
    {
        /// <summary>
        /// the coordinate of the tile in the tileset
        ///  the true positioning will be determined based off of a triangular coordinate system scaled to match the sprite sheet
        /// </summary>
        public TriangleCoordinate coords0;
        public string ID;
    }
    public struct MultiVertTileConfig
    {
        public string ID;
        public Vector2[] uvs;
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TriangleTileMap : MonoBehaviour
    {
        public TriangleTileMapTile[] tileTypes;
        public float sideLength;
        public float tilePadding;

        private Dictionary<string, MultiVertTileConfig> tileTypesDictionary;

        public TriangleCoordinateSystemBehavior coordSystem;
        public TriangleRhomboidCoordinateRange coordRange;

        public Dictionary<TriangleCoordinate, string> tiles;
        public string defaultTile;
        public string editTile;

        private GenericTileMapContainer<TriangleCoordinate> tileMapContainer;
        private ITileMapSystem<TriangleCoordinate> tileMapSystem;

        private void Awake()
        {
            var mainTex = GetComponent<MeshRenderer>().material.mainTexture;
            var textureSize = new Vector2(mainTex.width, mainTex.height);

            tileMapSystem = new TriangleTileMapSystem();
            tileMapContainer = new GenericTileMapContainer<TriangleCoordinate>(new TileTextureData
            {
                sideLength = sideLength,
                padding = tilePadding
            }, tileMapSystem);

            var convertedUVs = tileMapContainer.ConvertToStandardUVConfig(
                tileTypes.Select(x => new TileConfig<TriangleCoordinate>
                {
                    ID = x.ID,
                    tileCoordinate = x.coords0
                }),
                mainTex);
            tileTypesDictionary = convertedUVs.ToDictionary(x => x.ID);

            tiles = new Dictionary<TriangleCoordinate, string>();
            tiles[new TriangleCoordinate(0, 0, true)] = "ground";
            tiles[new TriangleCoordinate(0, 0, false)] = "ground";
            tiles[new TriangleCoordinate(1, 0, false)] = "ground";
            tiles[new TriangleCoordinate(0, 1, false)] = "ground";
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

                Debug.Log(coords);

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
        private Dictionary<TriangleCoordinate, int> coordinateCopyIndexes;

        private void SetupTilemap()
        {
            Mesh sourceMesh = new Mesh();
            sourceMesh.subMeshCount = 1;
            sourceMesh.SetVertices(
                TriangleCoordinateSystem.GetTriangleVertextesAround(new TriangleCoordinate(0, 0, false), 1)
                .Select(x => (Vector3)x)
                .ToList()
              );
            sourceMesh.SetColors(new Color[]
            {
                Color.white,
                Color.white,
                Color.white,
            });
            sourceMesh.SetTriangles(new int[]
            {
                0, 1, 2
            }, 0);
            sourceMesh.SetUVs(0, new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
            });

            coordinateCopyIndexes = new Dictionary<TriangleCoordinate, int>();

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
                var tileLocation = coordSystem.coordinateSystem.ToRealPosition(coord);

                Vector2[] uvs = tileConfig.uvs;

                var rotation = !coord.R ? 0f : 60f;
                rotation += Random.Range(0, 2) * 120f;

                var indexAdded = copier.NextCopy(tileLocation, UVOverride: uvs, localMeshRotation: Quaternion.Euler(0, 0, rotation));
                copier.CopySubmeshTrianglesToOffsetIndex(0, 0);

                coordinateCopyIndexes[coord] = indexAdded;
            }

            meshEditor = copier.FinalizeCopy();

            var meshHolder = GetComponent<MeshFilter>();
            meshHolder.mesh = targetMesh;
        }

    }
}
