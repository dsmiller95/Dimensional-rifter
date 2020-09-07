using Assets.Tiling;
using Assets.WorldObjects.SaveObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects.WorldGen
{
    public class RegionGenerator<T> where T : ICoordinate
    {
        private TileRegionSaveObjectTyped<T> mySaveObject;
        private MapGenerationConfiguration mapGenConfig;
        private System.Random randomGenerator;
        private Vector2 noiseOffset;

        private ICoordinateSystem<T> throwawayCoordinateSystem;
        public RegionGenerator(CoordinateSystemType type, ICoordinateRange<T> size, MapGenerationConfiguration config)
        {
            mapGenConfig = config;

            mySaveObject = new TileRegionSaveObjectTyped<T>();
            mySaveObject.tileType = type;
            mySaveObject.sideLength = 1f;
            mySaveObject.range = size;


            randomGenerator = new System.Random(mapGenConfig.seed);
            noiseOffset = new Vector2(
                (float)randomGenerator.NextDouble() * 1e6f,
                (float)randomGenerator.NextDouble() * 1e6f
                );
            throwawayCoordinateSystem = UniversalToGenericAdaptors.GetBasicCoordinateSystemFromType<T>(type);
        }


        public TileRegionSaveObjectTyped<T> GenerateSaveObject()
        {
            var memberSaveObject = new TileMembersSaveObject<T>();
            GenerateTiles();
            memberSaveObject.tiles = tiles.Select(x => new TileMapDataTile<T>
            {
                tileType = x.Value,
                coordinate = x.Key
            }).ToList();

            memberSaveObject.members = GenerateMemebers().ToList();

            memberSaveObject.defaultTile = mapGenConfig.defaultTile;

            mySaveObject.members = memberSaveObject;

            return mySaveObject;
        }

        private IDictionary<T, TileTypeInfo> tiles;
        private IEnumerable<TileMemberSaveObject<T>> GenerateMemebers()
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
                        .Select(coordinate => new TileMemberSaveObject<T>
                        {
                            coordinate = coordinate,
                            objectData = new TileMemberData
                            {
                                memberType = config.type.uniqueData,
                                objectDatas = config.type.InstantiateNewSaveObject()
                            }
                        })
                        .Take(config.amount)
                    );
        }

        private IEnumerable<T> GetInfiniteHaltonGeneratedCoordinatesInsideRange(ICoordinateRange<T> range)
        {
            var boundingPoints = range.BoundingPolygon(throwawayCoordinateSystem, mySaveObject.sideLength);
            var minVect = new Vector2(float.MaxValue, float.MaxValue);
            var maxVect = new Vector2(float.MinValue, float.MinValue);
            foreach (var extremePoint in boundingPoints)
            {
                minVect.x = Mathf.Min(extremePoint.x, minVect.x);
                minVect.y = Mathf.Min(extremePoint.y, minVect.y);
                maxVect.x = Mathf.Max(extremePoint.x, maxVect.x);
                maxVect.y = Mathf.Max(extremePoint.y, maxVect.y);
            }

            var haltonGenerator = new HaltonSequenceGenerator(2, 3, randomGenerator.Next(), maxVect, minVect);

            return haltonGenerator.InfiniteSample()
                .Select(x => throwawayCoordinateSystem.FromRealPosition(x))
                .Where(coord => range.ContainsCoordinate(coord));
        }

        private void GenerateTiles()
        {
            tiles = new Dictionary<T, TileTypeInfo>();
            foreach (var layer in mapGenConfig.tileGenLayers)
            {
                foreach (var coord in mySaveObject.range)
                {
                    var point = throwawayCoordinateSystem.ToRealPosition(coord);
                    var sample = SampleNoiseAtOctaves(layer.noise, point);
                    if (sample <= layer.cutoff)
                    {
                        tiles[coord] = layer.tileType;
                    }
                }
            }
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
