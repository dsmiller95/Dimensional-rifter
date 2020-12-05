using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    [Serializable]
    public struct PrefabAndType
    {
        public CoordinateRangeType type;
        public TileMapRegionPreview prefab;
    }
    [CreateAssetMenu(fileName = "TileMapPreviews", menuName = "TileMap/TileMapPreviews", order = 1)]
    public class TileMapPreviewsByCoordinateRangeType : ScriptableObject
    {
        public PrefabAndType[] prefabKeys;
        private IDictionary<CoordinateRangeType, PrefabAndType> _prefabs;
        public IDictionary<CoordinateRangeType, PrefabAndType> keyedPrefabs
        {
            get
            {
                if (_prefabs == null)
                    _prefabs = prefabKeys.ToDictionary(x => x.type);
                return _prefabs;
            }
        }
    }
}
