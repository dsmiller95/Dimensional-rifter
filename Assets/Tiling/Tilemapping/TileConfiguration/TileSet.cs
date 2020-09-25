using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.TileConfiguration
{

    [Serializable]
    public struct TileTypeInfo
    {
        public TileTypeInfo(string baseId, string shape)
        {
            baseID = baseId;
            shapeID = shape;
        }
        public string ID => baseID + shapeID;
        public string baseID;
        public string shapeID;
        public override bool Equals(object obj)
        {
            if (obj is TileTypeInfo other)
            {
                return other.baseID == baseID && other.shapeID == shapeID;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }

    public struct TileCoordinates
    {
        public UniversalCoordinate tileCoordinate;
        public TileTypeInfo typeIdentifier;
    }

    public abstract class TileSet : ScriptableObject
    {
        public float sideLength;
        public float tilePadding;

        public abstract IEnumerable<TileCoordinates> GetTileConfigs();
    }
}
