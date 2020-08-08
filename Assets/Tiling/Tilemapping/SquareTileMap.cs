using Assets.MapGen;
using Assets.Tiling.SquareCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Tiling.Tilemapping
{
    [Serializable]
    public struct SquareTileMapTile
    {
        public Vector2 coords0;
        public Vector2 coords1;
        public string ID;
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class SquareTileMap : MonoBehaviour
    {
        public SquareTileMapTile[] tileTypes;
        private Dictionary<string, SquareTileMapTileInternal> tileTypesDictionary;

        public SquareCoordinateSystemBehavior coordSystem;
        public SquareCoordinateRange coordRange;

        public Dictionary<SquareCoordinate, string> tiles;
        public string defaultTile;
        public string editTile;


        public struct SquareTileMapTileInternal
        {
            public Vector2 uv00;
            public Vector2 uv01;
            public Vector2 uv11;
            public Vector2 uv10;
            public string ID;
        }

        private void Awake()
        {
            var mat = this.GetComponent<MeshRenderer>().material;
            var textureSize = new Vector2(mat.mainTexture.width, mat.mainTexture.height);

            tileTypesDictionary = tileTypes.ToDictionary(x => x.ID, tileType =>
            {
                var uv00 = new Vector2(tileType.coords0.x / textureSize.x, tileType.coords0.y / textureSize.y);
                var uv11 = new Vector2(tileType.coords1.x / textureSize.x, tileType.coords1.y / textureSize.y);
                return new SquareTileMapTileInternal
                {
                    uv00 = uv00,
                    uv11 = uv11,
                    uv01 = new Vector2(uv00.x, uv11.y),
                    uv10 = new Vector2(uv11.x, uv00.y),
                    ID = tileType.ID
                };
                
            });
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

                if(coordinateCopyIndexes.TryGetValue(coords, out var index))
                {
                    if (tileTypesDictionary.TryGetValue(editTile, out var tileconfig))
                    {
                        var uvs = new Vector2[]
                        {
                            tileconfig.uv00,
                            tileconfig.uv01,
                            tileconfig.uv11,
                            tileconfig.uv10,
                        };
                        meshEditor.SetUVForVertexesAtDuplicate(index, uvs);
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
            sourceMesh.SetVertices(new Vector3[] {
                new Vector3(-.5f,-.5f),
                new Vector3(-.5f, .5f),
                new Vector3( .5f, .5f),
                new Vector3( .5f,-.5f),});
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

            foreach(var coord in coordRange)
            {
                string tileType;
                if(!tiles.TryGetValue(coord, out tileType))
                {
                    tileType = defaultTile;
                }

                var tileConfig = tileTypesDictionary[tileType];
                var tileLocation = rectCoords.ToRealPosition(coord);

                var uvs = new Vector2[]
                {
                    tileConfig.uv00,
                    tileConfig.uv01,
                    tileConfig.uv11,
                    tileConfig.uv10,
                };

                var indexAdded = copier.NextCopy(tileLocation, UVOverride: uvs);
                copier.CopySubmeshTrianglesToOffsetIndex(0, 0);

                coordinateCopyIndexes[coord] = indexAdded;
            }

            meshEditor = copier.FinalizeCopy();

            var meshHolder = GetComponent<MeshFilter>();
            meshHolder.mesh = targetMesh;
        }

    }
}
