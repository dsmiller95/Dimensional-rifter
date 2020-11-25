using Assets.Tiling;
using Assets.Tiling.Tilemapping.TileConfiguration;
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
        public int memberID;
        public InMemberObjectData[] objectDatas;
    }

    [Serializable]
    public struct TileMemberSaveObject
    {
        public UniversalCoordinate coordinate;
        public TileMemberData objectData;
    }
    [Serializable]
    public class UniversalTileMembersSaveObject
    {
        public IList<TileMemberSaveObject> members;
        public TileTypeInfo defaultTile;

        public UniversalCoordinate[] tileKeys;
        public int[] tileValues;
        public TileTypeInfo[] tileTypeInfoByIndex;
    }
}
