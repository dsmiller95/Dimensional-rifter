using Assets.MapGen;
using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public struct MultiVertTileConfig
    {
        public string ID;
        public Vector2[] uvs;
    }

    /// <summary>
    /// Responsible for: Storing data about which tiles are where on the map
    ///     And generating the tileMap mesh. Exposes methods to select which coordinates
    ///     to bake into the mesh, as well as methods which can be used to quickly visually remove
    ///     tiles from the mesh for higher-frequency calls
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TileMapMeshBuilder<T> where T : ICoordinate
    {
        private TileSet<T> tileSet;
        private TileMapTileShapeStrategy<T> tileMapSystem;
        private CoordinateSystemMembers<T> coordinateMemebers;

        public TileMapMeshBuilder(TileSet<T> tileSet, TileMapTileShapeStrategy<T> tileMappingSystem, CoordinateSystemMembers<T> members)
        {
            this.tileSet = tileSet;
            tileMapSystem = tileMappingSystem;
            this.coordinateMemebers = members;
        }

        private IDictionary<string, MultiVertTileConfig> tileTypesDictionary;

        public void SetupTilesOnGivenTexture(
            Texture textureToSample)
        {
            var textureSize = new Vector2(textureToSample.width, textureToSample.height);

            var trueSideLength = tileSet.sideLength + tileSet.tilePadding * 2;

            ICoordinateSystem<T> textureSpaceCoordinateSystem = new CoordinateSystemScale<T>(tileMapSystem.GetBasicCoordinateSystem(), Vector2.one * trueSideLength);

            var uv0s = tileMapSystem.GetVertexesAround(textureSpaceCoordinateSystem.DefaultCoordinate(), trueSideLength, textureSpaceCoordinateSystem);
            var textureSpaceOrigin = uv0s.First();

            tileTypesDictionary = tileSet.GetTileConfigs().Select(tileType =>
            {
                var result = new MultiVertTileConfig { ID = tileType.typeIdentifier.ID };

                var uvsInTextureSpace = tileMapSystem.GetVertexesAround(tileType.tileCoordinate, tileSet.sideLength, textureSpaceCoordinateSystem);

                result.uvs = uvsInTextureSpace
                    .Select(x => x - textureSpaceOrigin)
                    .Select(x => x.InverseScale(textureSize))
                    .ToArray();

                return result;
            }).ToDictionary(x => x.ID);
        }

        public void SetTileEnabled(T coordinate, bool enabled)
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

        public IEnumerable<T> GetBakedTiles()
        {
            return coordinateCopyIndexes.Keys;
        }
        public void SetMeshForTileToType(T coordinate, TileTypeInfo tileID)
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
        private Dictionary<T, int> coordinateCopyIndexes;
        private ISet<T> disabledCoordinates;

        public Mesh BakeTilemapMesh(
            ICoordinateRange<T> range,
            ICoordinateSystem<T> tilePlacementSystem,
            Func<T, Vector2, bool> tileFilter)
        {
            Mesh sourceMesh = new Mesh();
            sourceMesh.subMeshCount = 1;
            var basicCoordSystem = tileMapSystem.GetBasicCoordinateSystem();
            var defaultVerts = tileMapSystem.GetVertexesAround(basicCoordSystem.DefaultCoordinate(), 1)
                .Select(x => (Vector3)x)
                .ToList();
            sourceMesh.SetVertices(
                defaultVerts
              );
            sourceMesh.SetColors(defaultVerts.Select(x => Color.white).ToList());
            sourceMesh.SetTriangles(tileMapSystem.GetTileTriangles(), 0);
            // uvs don't matter -- we'll be overwriting all of these
            sourceMesh.SetUVs(0, defaultVerts.Select(x => default(Vector2)).ToList());

            coordinateCopyIndexes = new Dictionary<T, int>();

            var targetMesh = new Mesh();
            var copier = new MeshCopier(sourceMesh, 1, targetMesh, 1);

            foreach (var coord in range)
            {
                var tileLocation = tilePlacementSystem.ToRealPosition(coord);
                if (!tileFilter(coord, tileLocation))
                {
                    continue;
                }

                string tileTypeId = coordinateMemebers.GetTileType(coord).ID;

                MultiVertTileConfig tileConfig;
                if(!tileTypesDictionary.TryGetValue(tileTypeId, out tileConfig))
                {
                    Debug.LogError("help");
                }

                //var tileConfig = tileTypesDictionary[tileTypeId];

                Vector2[] uvs = tileConfig.uvs;

                var vertexes = tileMapSystem.GetVertexesAround(coord, 1, tilePlacementSystem).Select(x => (Vector3)x);
                var indexAdded = copier.NextCopy(tileLocation, UVOverride: uvs, vertexOverrides: vertexes);
                copier.CopySubmeshTrianglesToOffsetIndex(0, 0);

                coordinateCopyIndexes[coord] = indexAdded;
            }

            meshEditor = copier.FinalizeCopy();

            disabledCoordinates = new HashSet<T>();

            return targetMesh;
        }
    }
}
