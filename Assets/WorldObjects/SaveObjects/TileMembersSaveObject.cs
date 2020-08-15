using Assets.Tiling;
using Assets.WorldObjects.Members;
using System;
using System.Collections.Generic;

namespace Assets.WorldObjects.SaveObjects
{
    [Serializable]
    public struct TileMemberData
    {
        public MemberType memberType;
        public object memberData;
    }

    [Serializable]
    public struct TileMemberSaveObject<T>
    {
        public T coordinate;
        public TileMemberData objectData;
    }
    [Serializable]
    public struct TileMapDataTile<T>
    {
        public T coordinate;
        public TileTypeInfo tileType;
    }
    [Serializable]
    public class TileMembersSaveObject<T> where T : ICoordinate
    {
        public IList<TileMemberSaveObject<T>> members;
        public IList<TileMapDataTile<T>> tiles;
        public TileTypeInfo defaultTile;
    }
}
