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

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class TriangleTileMap : MonoBehaviour
    {
        public TriangleTileMapTile[] tileTypes;
        public float sideLength;
        public float tilePadding;

        private Dictionary<string, TriangleTileMapTileInternal> tileTypesDictionary;

        public TriangleCoordinateSystemBehavior coordSystem;
        public TriangleRhomboidCoordinateRange coordRange;

        public Dictionary<TriangleCoordinate, string> tiles;
        public string defaultTile;
        public string editTile;

        //private static readonly float triangleHeightBySideLength = Mathf.Sqrt(3) / 2f;

        public struct TriangleTileMapTileInternal
        {
            /// <summary>
            /// uvs will always start from left-most coord and rotate clockwise from there
            /// </summary>
            public Vector2 uv0;
            public Vector2 uv1;
            public Vector2 uv2;
            public string ID;
        }

        private void Awake()
        {
            var mat = GetComponent<MeshRenderer>().material;
            var textureSize = new Vector2(mat.mainTexture.width, mat.mainTexture.height);

            var trueSideLength = sideLength + tilePadding * 2;

            var textureSpaceCoordinateSystem = new CoordinateSystemScale<TriangleCoordinate>(new TriangleCoordinateSystem(), Vector2.one * trueSideLength);
            var pos0 = textureSpaceCoordinateSystem.ToRealPosition(new TriangleCoordinate(0, 0, false));

            var uv0s = GetTriangleVertextes(new TriangleCoordinate(0, 0, false), textureSpaceCoordinateSystem, trueSideLength);

            var textureSpaceOrigin = uv0s.First();

            tileTypesDictionary = tileTypes.ToDictionary(x => x.ID, tileType =>
            {
                var result = new TriangleTileMapTileInternal { ID = tileType.ID };

                var uvsInTextureSpace = GetTriangleVertextes(tileType.coords0, textureSpaceCoordinateSystem, sideLength).ToArray();

                result.uv0 = uvsInTextureSpace[0] - textureSpaceOrigin;
                result.uv1 = uvsInTextureSpace[1] - textureSpaceOrigin;
                result.uv2 = uvsInTextureSpace[2] - textureSpaceOrigin;
                //result.uv0 = tileType.coords0;
                //if (tileType.flatTop)
                //{
                //    result.uv1 = result.uv0 + Vector2.right * sideLength;
                //    result.uv2 = result.uv0 + Vector2.right * (sideLength / 2f) + triHeight;
                //}else
                //{
                //    result.uv1 = result.uv0 + Vector2.right * (sideLength / 2f) - triHeight;
                //    result.uv2 = result.uv0 + Vector2.right * sideLength;
                //}

                // transform from coords in texture space to coords in UV space
                result.uv0 = result.uv0.InverseScale(textureSize);
                result.uv1 = result.uv1.InverseScale(textureSize);
                result.uv2 = result.uv2.InverseScale(textureSize);

                // UV coords pivot from lower-left
                // but pixel coords are input based on (0,0) in upper-left
                // so invert the UV to transform into UV-space
                //result.uv0.y = 1 - result.uv0.y;
                //result.uv1.y = 1 - result.uv1.y;
                //result.uv2.y = 1 - result.uv2.y;

                return result;
            });
            tiles = new Dictionary<TriangleCoordinate, string>();
            tiles[new TriangleCoordinate(0, 0, true)] = "ground";
            tiles[new TriangleCoordinate(0, 0, false)] = "ground";
            tiles[new TriangleCoordinate(1, 0, false)] = "ground";
            tiles[new TriangleCoordinate(0, 1, false)] = "ground";
        }

        private static readonly Vector2[] triangleVerts = new Vector2[] {
                new Vector3(-.5f,-1/(Mathf.Sqrt(3) * 2)),
                new Vector3(  0f, 1/Mathf.Sqrt(3)),
                new Vector3( .5f, -1/(Mathf.Sqrt(3) * 2)) };
        private static IEnumerable<Vector2> GetTriangleVertextes(TriangleCoordinate coord, ICoordinateSystem<TriangleCoordinate> coordSystem, float triangleScale)
        {
            IEnumerable<Vector2> verts = triangleVerts;
            if (coord.R)
            {
                var rotation = Quaternion.Euler(0, 0, 60);
                verts = verts.Select(x => (Vector2)(rotation * x));
            }
            var location = coordSystem.ToRealPosition(coord);
            return verts.Select(x => (x * triangleScale) + location);
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
                        var uvs = new Vector2[]
                        {
                            tileconfig.uv0,
                            tileconfig.uv1,
                            tileconfig.uv2,
                        };
                        meshEditor.SetUVForVertexesAtDuplicate(index, uvs);
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
            sourceMesh.SetVertices(new Vector3[] {
                new Vector3(-.5f,-1/(Mathf.Sqrt(3) * 2)),
                new Vector3(  0f, 1/Mathf.Sqrt(3)),
                new Vector3( .5f, -1/(Mathf.Sqrt(3) * 2)),});
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

            ICoordinateSystem<TriangleCoordinate> rectCoords = coordSystem.coordinateSystem;
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
                var tileLocation = rectCoords.ToRealPosition(coord);

                Vector2[] uvs = new Vector2[]
                     {
                        tileConfig.uv0,
                        tileConfig.uv1,
                        tileConfig.uv2,
                     };

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
