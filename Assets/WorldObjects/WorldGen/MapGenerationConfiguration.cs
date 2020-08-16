using Assets.Tiling.Tilemapping.TileConfiguration;
using System;
using UnityEngine;

namespace Assets.WorldObjects.WorldGen
{

    [Serializable]
    public struct NoiseOctave
    {
        public Vector2 frequency;
        public float weight;
    }

    [Serializable]
    public struct TileTypeLayer
    {
        public NoiseOctave[] noise;
        public float cutoff;
        public TileTypeInfo tileType;
    }

    [CreateAssetMenu(fileName = "MapGenerator", menuName = "MapGeneration/GenerationParameters", order = 1)]
    public class MapGenerationConfiguration : ScriptableObject
    {
        public TileTypeLayer[] tileGenLayers;
        public TileTypeInfo defaultTile;

        public TileDefinitions tileDefinitions;

        public Vector2Int baseMapSize;
        public int seed;
    }
}
