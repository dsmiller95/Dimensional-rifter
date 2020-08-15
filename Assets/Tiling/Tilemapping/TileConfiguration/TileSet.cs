using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.TileConfiguration
{
    public struct TileConfig<T> where T : ICoordinate
    {
        public T tileCoordinate;
        public string ID;
    }

    public abstract class TileSet<T> : ScriptableObject where T : ICoordinate
    {
        public float sideLength;
        public float tilePadding;

        public abstract IEnumerable<TileConfig<T>> GetTileConfigs();
    }
}
