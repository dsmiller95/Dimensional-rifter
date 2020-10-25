using Assets.Tiling.Tilemapping.TileConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.MeshEdit
{
    /// <summary>
    /// UV data about a type of tile
    /// </summary>
    public struct MultiVertTileConfig
    {
        /// <summary>
        /// unique tile type and shape identifier
        /// </summary>
        public string ID;
        /// <summary>
        /// a list of the UVs used to map this tile to the texture
        /// </summary>
        public Vector2[] uvs;
    }

    /// <summary>
    /// Responsible for: Storing data about which tiles are where on the map
    ///     And generating the tileMap mesh. Exposes methods to select which coordinates
    ///     to bake into the mesh, as well as methods which can be used to quickly visually remove
    ///     tiles from the mesh for higher-frequency calls
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TileMapMeshBuilder
    {
        private TileSet tileSet;
        private UniversalCoordinateSystemMembers coordinateMemebers;

        public TileMapMeshBuilder(TileSet tileSet, UniversalCoordinateSystemMembers members)
        {
            this.tileSet = tileSet;
            coordinateMemebers = members;
        }

        private IDictionary<string, MultiVertTileConfig> tileTypesDictionary;

        public void SetupTilesForGivenTexture(
            Texture textureToSample)
        {
            var textureSize = new Vector2(textureToSample.width, textureToSample.height);
            var trueSideLength = tileSet.sideLength + tileSet.tilePadding * 2;
            var textureSpaceScaling = Vector2.one * trueSideLength;

            Vector2 minUV = Vector2.one * 1000;

            tileTypesDictionary = tileSet.GetTileConfigs().Select(tileType =>
                {
                    var result = new MultiVertTileConfig { ID = tileType.typeIdentifier.ID };

                    result.uvs = tileType.tileCoordinate.GetVertexesAround()
                        .Select(vert => vert * trueSideLength)
                        .ToArray();

                    foreach (var uv in result.uvs)
                    {
                        minUV.x = Mathf.Min(minUV.x, uv.x);
                        minUV.y = Mathf.Min(minUV.y, uv.y);
                    }

                    return result;
                })
                .ToList()
                .Select(tileConf =>
                {
                    for (var i = 0; i < tileConf.uvs.Length; i++)
                    {
                        tileConf.uvs[i] = (tileConf.uvs[i] - minUV).InverseScale(textureSize);
                    }

                    return tileConf;
                })
                .ToDictionary(x => x.ID);
        }

        [Obsolete("No longer used, nothing edits the tilemap. may not function correctly")]
        public void SetTileEnabled(UniversalCoordinate coordinate, bool enabled)
        {
            var isTileDisabled = disabledCoordinates.Contains(coordinate);
            if (enabled == !isTileDisabled)
            {
                return;
            }
            if (coordinateCopyIndexes.TryGetValue(coordinate, out var index))
            {
                if (isTileDisabled && enabled)
                {
                    meshEditor.EnableGeometryAtDuplicate(index);
                    disabledCoordinates.Remove(coordinate);
                }
                else if (!isTileDisabled && !enabled)
                {
                    meshEditor.DisableGeometryAtDuplicate(index);
                    disabledCoordinates.Add(coordinate);
                }
            }
        }

        [Obsolete("No longer used, nothing edits the tilemap. may not function correctly")]
        public IEnumerable<UniversalCoordinate> GetBakedTiles()
        {
            return coordinateCopyIndexes.Keys;
        }
        [Obsolete("No longer used, nothing edits the tilemap. may not function correctly")]
        public void SetMeshForTileToType(UniversalCoordinate coordinate, TileTypeInfo tileID)
        {
            if (coordinateCopyIndexes != null && coordinateCopyIndexes.TryGetValue(coordinate, out var index))
            {
                if (tileTypesDictionary.TryGetValue(tileID.ID, out var tileconfig))
                {
                    meshEditor.SetUVForVertexesAtDuplicate(index, tileconfig.uvs);
                }
            }
        }

        private CopiedMeshEditor meshEditor;
        private Dictionary<UniversalCoordinate, int> coordinateCopyIndexes;
        private ISet<UniversalCoordinate> disabledCoordinates;

        public Mesh BakeTilemapMesh(
            IUniversalCoordinateRange range,
            Func<UniversalCoordinate, Vector2, bool> tileFilter)
        {
            Mesh sourceMesh = new Mesh();
            sourceMesh.subMeshCount = 1;
            var defaultCoord = UniversalCoordinate.GetDefault(range.coordinateType);
            var defaultVerts = defaultCoord.GetVertexesAround()
                .Select(x => (Vector3)x)
                .ToList();
            sourceMesh.SetVertices(
                defaultVerts
              );
            sourceMesh.SetColors(defaultVerts.Select(x => Color.white).ToList());
            sourceMesh.SetTriangles(UniversalCoordinate.GetTileTriangleIDs(range.coordinateType), 0);
            // uvs don't matter -- we'll be overwriting all of these
            sourceMesh.SetUVs(0, defaultVerts.Select(x => default(Vector2)).ToList());

            coordinateCopyIndexes = new Dictionary<UniversalCoordinate, int>();

            var targetMesh = new Mesh();
            if (range.TotalCoordinateContents() * defaultVerts.Count < 65000)
            {
                // TODO: optimize mesh building/rendering so we don't need this
                targetMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            var copier = new MeshCopier(sourceMesh, 1, targetMesh, 1);

            foreach (var coord in range.GetUniversalCoordinates())
            {
                var tileLocation = coord.ToPositionInPlane();
                if (!tileFilter(coord, tileLocation))
                {
                    continue;
                }

                string tileTypeId = coordinateMemebers.GetTileType(coord).ID;

                MultiVertTileConfig tileConfig;
                if (!tileTypesDictionary.TryGetValue(tileTypeId, out tileConfig))
                {
                    Debug.LogError("help");
                }

                //var tileConfig = tileTypesDictionary[tileTypeId];

                Vector2[] uvs = tileConfig.uvs;

                var vertexes = coord.GetVertexesAround().Select(x => (Vector3)x);
                var indexAdded = copier.NextCopy(tileLocation, UVOverride: uvs, vertexOverrides: vertexes);
                copier.CopySubmeshTrianglesToOffsetIndex(0, 0);

                coordinateCopyIndexes[coord] = indexAdded;
            }

            meshEditor = copier.FinalizeCopy();

            disabledCoordinates = new HashSet<UniversalCoordinate>();

            return targetMesh;
        }
    }
}
