using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.WorldObjects.SaveObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects.WorldGen
{
    public class RegionGenerator
    {
        private TileRegionSaveObject mySaveObject;

        private MapGenerationConfiguration mapGenConfig;
        private System.Random randomGenerator;
        private Vector2 noiseOffset;

        public RegionGenerator(
            UniversalCoordinateRange size,
            MapGenerationConfiguration config)
        {
            mapGenConfig = config;

            mySaveObject = new TileRegionSaveObject();
            mySaveObject.range = size;


            randomGenerator = new System.Random(mapGenConfig.seed);
            noiseOffset = new Vector2(
                (float)randomGenerator.NextDouble() * 1e6f,
                (float)randomGenerator.NextDouble() * 1e6f
                );
        }


        public TileRegionSaveObject GenerateSaveObject(UniversalTileMembersSaveObject everyMemberObject)
        {
            var tiles = GenerateTiles();
            var tileKeyValues = tiles.ToArray();

            everyMemberObject.tileKeys = everyMemberObject.tileKeys.Concat(
                    tiles.Select(x => x.Key)
                ).ToArray();
            var tiledataIndexed = everyMemberObject.tileTypeInfoByIndex;
            var maxTileIndex = tiledataIndexed.Length - 1;
            var indexByTileData = tiledataIndexed.Select((key, i) => new KeyValuePair<TileTypeInfo, int>(key, i)).ToDictionary(x => x.Key, x => x.Value);
            everyMemberObject.tileValues = everyMemberObject.tileValues.Concat(
                    tiles.Select(x =>
                    {
                        var tileInfo = x.Value;
                        if (indexByTileData.TryGetValue(tileInfo, out var index))
                        {
                            return index;
                        }
                        maxTileIndex++;
                        indexByTileData[tileInfo] = maxTileIndex;
                        return maxTileIndex;
                    })
                ).ToArray();
            var newTileTypeInfo = new TileTypeInfo[maxTileIndex + 1];
            foreach (var pair in indexByTileData)
            {
                newTileTypeInfo[pair.Value] = pair.Key;
            }
            everyMemberObject.tileTypeInfoByIndex = newTileTypeInfo;

            everyMemberObject.members = everyMemberObject.members.Concat(GenerateMemebers(tiles)).ToList();

            return mySaveObject;
        }

        private IEnumerable<TileMemberSaveObject> GenerateMemebers(IDictionary<UniversalCoordinate, TileTypeInfo> tiles)
        {
            var coordinateGenerator = GetInfiniteHaltonGeneratedCoordinatesInsideRange(mySaveObject.range)
                .Where(coordinate =>
                {
                    TileTypeInfo tile;
                    if (!tiles.TryGetValue(coordinate, out tile))
                    {
                        tile = mapGenConfig.defaultTile;
                    }
                    var tileProps = mapGenConfig.tileDefinitions.GetTileProperties(tile);
                    return tileProps.isPassable;
                });
            return mapGenConfig.memberGenerationOptions.SelectMany(config => coordinateGenerator
                        .Select(coordinate => new TileMemberSaveObject
                        {
                            coordinate = coordinate,
                            objectData = new TileMemberData
                            {
                                memberID = config.type.memberID,
                                objectDatas = config.type.InstantiateNewSaveObject()
                            }
                        })
                        .Take(config.amount)
                    );
        }

        private IEnumerable<UniversalCoordinate> GetInfiniteHaltonGeneratedCoordinatesInsideRange(UniversalCoordinateRange range)
        {
            // TODO: do actual clustering, with a random cluster origin
            //var boundingPoints = range.BoundingPolygon();
            //var minVect = new Vector2(float.MaxValue, float.MaxValue);
            //var maxVect = new Vector2(float.MinValue, float.MinValue);
            //foreach (var extremePoint in boundingPoints)
            //{
            //    minVect.x = Mathf.Min(extremePoint.x, minVect.x);
            //    minVect.y = Mathf.Min(extremePoint.y, minVect.y);
            //    maxVect.x = Mathf.Max(extremePoint.x, maxVect.x);
            //    maxVect.y = Mathf.Max(extremePoint.y, maxVect.y);
            //}

            var minVect = Vector2.one * -1 * mapGenConfig.memberClusterSizeAtCenter;
            var maxVect = Vector2.one * 1 * mapGenConfig.memberClusterSizeAtCenter;

            var haltonGenerator = new HaltonSequenceGenerator(2, 3, randomGenerator.Next(), maxVect, minVect);
            var type = range.CoordinateType;
            return haltonGenerator.InfiniteSample()
                .Select(x => UniversalCoordinate.FromPositionInPlane(x, type, mySaveObject.range.CoordinatePlaneID))
                .Where(coord => range.ContainsCoordinate(coord));
        }

        private IDictionary<UniversalCoordinate, TileTypeInfo> GenerateTiles()
        {
            var tiles = new Dictionary<UniversalCoordinate, TileTypeInfo>();
            foreach (var layer in mapGenConfig.tileGenLayers)
            {
                foreach (var coord in mySaveObject.range.GetUniversalCoordinates())
                {
                    var point = coord.ToPositionInPlane();
                    var sample = SampleNoiseAtOctaves(layer.noise, point);
                    if (sample <= layer.cutoff)
                    {
                        tiles[coord] = layer.tileType;
                    }
                }
            }
            return tiles;
        }

        private float SampleNoiseAtOctaves(NoiseOctave[] octaves, Vector2 point)
        {
            var perlinVector = point + noiseOffset;

            var sample = 0f;
            foreach (var octave in octaves)
            {
                sample += SamplePerlin(perlinVector, octave);
            }

            return sample / octaves.Sum(octave => octave.weight);
        }
        public float SamplePerlin(Vector2 point, NoiseOctave octave)
        {
            var sampleVector = Vector2.Scale(point, octave.frequency);
            return Mathf.PerlinNoise(sampleVector.x, sampleVector.y) * octave.weight;
        }
    }
}
