using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.WorldObjects.Members;
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

    [Serializable]
    public struct TileMemberGeneration
    {
        public MemberType type;
        public int amount;
    }

    [CreateAssetMenu(fileName = "MapGenerator", menuName = "MapGeneration/GenerationParameters", order = 1)]
    public class MapGenerationConfiguration : ScriptableObject
    {
        public TileTypeLayer[] tileGenLayers;
        public TileTypeInfo defaultTile;

        public TileMemberGeneration[] memberGenerationOptions;
        [Tooltip("Temporary utility to force all the members to generate in a cluster roughly this wide")]
        public float memberClusterSizeAtCenter = 10;

        public TileDefinitions tileDefinitions;

        public Vector2Int baseMapSize;
        public int seed;
    }
}
