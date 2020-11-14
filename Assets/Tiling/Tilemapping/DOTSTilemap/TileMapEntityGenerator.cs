using Assets.Tiling.Tilemapping.MeshEdit;
using Assets.Tiling.Tilemapping.TileConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.DOTSTilemap
{
    [Obsolete("This approach is significantly slower than building a single large mesh")]
    public class TileMapEntityGenerator
    {
        private TileSet tileSet;
        private UniversalCoordinateSystemMembers coordinateMemebers;
        private Material tileMaterial;

        public TileMapEntityGenerator(
            Material tileMaterial,
            TileSet tileSet,
            UniversalCoordinateSystemMembers members)
        {
            this.tileMaterial = tileMaterial;
            this.tileSet = tileSet;
            coordinateMemebers = members;
        }

        private IDictionary<string, MultiVertTileConfig> tileTypesDictionary;

        public void SetupTilesForOwnMaterial()
        {
            var textureToSample = tileMaterial.mainTexture;
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

        private class MeshRendererCacheableData
        {
            public Mesh mesh;
            public AABB meshBounds;
        }

        public void CreateAllEntitiesForTilemap(
            UniversalCoordinateRange range,
            Func<UniversalCoordinate, Vector2, bool> tileFilter)
        {
            var meshDataByTileType = new Dictionary<string, MeshRendererCacheableData>();

            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var tileArchetype = manager.CreateArchetype(
                typeof(Translation),
                typeof(LocalToWorld),
                typeof(RenderMesh),
                typeof(RenderBounds)
                );


            var defaultCoord = UniversalCoordinate.GetDefault(range.CoordinateType);
            var defaultVerts = defaultCoord.GetVertexesAround()
                .Select(x => (Vector3)x)
                .ToArray();

            var defaultTriangles = UniversalCoordinate.GetTileTriangleIDs(range.CoordinateType);

            NativeArray<Entity> newTiles = new NativeArray<Entity>(range.TotalCoordinateContents(), Allocator.Temp);
            manager.CreateEntity(tileArchetype, newTiles);

            var currentTileIndex = 0;
            foreach (var coord in range.GetUniversalCoordinates())
            {
                var tileLocation = coord.ToPositionInPlane();
                if (!tileFilter(coord, tileLocation))
                {
                    continue;
                }

                string tileTypeId = coordinateMemebers.GetTileType(coord).ID;

                if (!meshDataByTileType.TryGetValue(tileTypeId, out var tileMeshData))
                {
                    if (!tileTypesDictionary.TryGetValue(tileTypeId, out var tileConfig))
                    {
                        throw new Exception("Missing tile configuration value, cannot continue building tilemap");
                    }
                    else
                    {
                        var newMesh = GenerateMeshFromConfig(tileConfig, defaultVerts, defaultTriangles);
                        var newMeshData = new MeshRendererCacheableData
                        {
                            mesh = newMesh,
                            meshBounds = newMesh.bounds.ToAABB()
                        };
                        meshDataByTileType[tileTypeId] = newMeshData;
                        tileMeshData = newMeshData;
                    }
                }

                var tileEntity = newTiles[currentTileIndex];
                manager.SetComponentData(tileEntity,
                    new Translation
                    {
                        Value = new float3(tileLocation, 0)
                    });

                manager.SetSharedComponentData(tileEntity,
                    new RenderMesh
                    {
                        material = tileMaterial,
                        mesh = tileMeshData.mesh
                    });

                currentTileIndex++;
            }
        }

        private Mesh GenerateMeshFromConfig(MultiVertTileConfig tileConfig, Vector3[] vertexes, int[] defaultTriangles)
        {

            Mesh newMesh = new Mesh();
            newMesh.subMeshCount = 1;
            newMesh.SetVertices(vertexes);
            newMesh.SetTriangles(defaultTriangles, 0);
            // uvs don't matter -- we'll be overwriting all of these
            newMesh.SetUVs(0, tileConfig.uvs);

            return newMesh;
        }
    }
}
