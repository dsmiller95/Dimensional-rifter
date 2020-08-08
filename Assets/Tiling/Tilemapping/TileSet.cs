﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public abstract class TileSet<T> : MonoBehaviour where T : ICoordinate
    {
        public float sideLength;
        public float tilePadding;

        public abstract IEnumerable<TileConfig<T>> GetTileConfigs();
    }
}
