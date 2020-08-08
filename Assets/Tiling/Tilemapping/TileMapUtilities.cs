using Assets.Tiling.Tilemapping.Triangle;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public struct TileTextureData
    {
        public float sideLength;
        public float padding;
    }
    public struct TileConfig<T> where T : ICoordinate
    {
        public T tileCoordinate;
        public string ID;
    }

    public class GenericTileMapContainer<T> where T : ICoordinate
    {

        private TileTextureData textureConfig;
        private ITileMapSystem<T> tileMapSystem;

        public GenericTileMapContainer(TileTextureData textureData, ITileMapSystem<T> tileMappingSystem)
        {
            textureConfig = textureData;
            tileMapSystem = tileMappingSystem;
        }


        public MultiVertTileConfig[] ConvertToStandardUVConfig(
            IEnumerable<TileConfig<T>> tiles,
            Texture textureToSample)
        {
            var textureSize = new Vector2(textureToSample.width, textureToSample.height);

            var trueSideLength = textureConfig.sideLength + textureConfig.padding * 2;

            ICoordinateSystem<T> textureSpaceCoordinateSystem = new CoordinateSystemScale<T>(tileMapSystem.GetBasicCoordinateSystem(), Vector2.one * trueSideLength);

            var uv0s = tileMapSystem.GetVertexesAround(textureSpaceCoordinateSystem.DefaultCoordinate(), trueSideLength, textureSpaceCoordinateSystem);
            var textureSpaceOrigin = uv0s.First();

            return tiles.Select(tileType =>
            {
                var result = new MultiVertTileConfig { ID = tileType.ID };

                var uvsInTextureSpace = tileMapSystem.GetVertexesAround(tileType.tileCoordinate, textureConfig.sideLength, textureSpaceCoordinateSystem);

                result.uvs = uvsInTextureSpace
                    .Select(x => x - textureSpaceOrigin)
                    .Select(x => x.InverseScale(textureSize))
                    .ToArray();

                return result;
            }).ToArray();
        }
    }
}
