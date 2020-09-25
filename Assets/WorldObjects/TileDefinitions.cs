using Assets.Tiling.Tilemapping.TileConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects
{
    [Serializable]
    public struct TileProperties
    {
        public string tileBaseID;
        public bool isPassable;
    }
    [CreateAssetMenu(fileName = "TileDefinition", menuName = "MapGeneration/TileDefinitionSet", order = 2)]
    public class TileDefinitions : ScriptableObject
    {
        public TileProperties[] propertyDefinitions;

        private IDictionary<string, TileProperties> properties;

        public TileProperties GetTileProperties(TileTypeInfo tileType)
        {
            if (properties == null)
            {
                properties = propertyDefinitions.ToDictionary(x => x.tileBaseID);
            }
            return properties[tileType.baseID];
        }
    }
}
