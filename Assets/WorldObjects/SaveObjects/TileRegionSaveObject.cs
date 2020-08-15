using Assets.Tiling;
using System;

namespace Assets.WorldObjects.SaveObjects
{
    [Serializable]
    public class TileRegionSaveObject
    {
        public CoordinateSystemType tileType;
        public float sideLength;
    }

    [Serializable]
    public class TileRegionSaveObjectTyped<T> : TileRegionSaveObject where T : ICoordinate
    {
        public ICoordinateRange<T> range;
        public TileMembersSaveObject<T> members;
    }
}
