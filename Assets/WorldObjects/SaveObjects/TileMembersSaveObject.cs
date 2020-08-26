using Assets.Tiling;
using Assets.WorldObjects.Members;
using System;
using System.Collections.Generic;

namespace Assets.WorldObjects.SaveObjects
{
    [Serializable]
    public struct InMemberObjectData
    {
        public string identifierInMember;
        public object data;
    }

    [Serializable]
    public struct TileMemberData
    {
        public MemberTypeUniqueData memberType;
        public InMemberObjectData[] objectDatas;
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
