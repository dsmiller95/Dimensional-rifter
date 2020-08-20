using Assets.Tiling;
using Assets.WorldObjects.Members;
using Assets.WorldObjects.SaveObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects
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

    public class CoordinateSystemMembersAllCoordinates : MonoBehaviour
    {
        public IEnumerable<TileMapMember> allMembers => members;
        public TileTypeInfo defaultTile;

        public MemberPrefabRegistry memberPrefabRegistry;

        protected ISet<TileMapMember> members;
        public CoordinateSystemMembersAllCoordinates()
        {
            members = new HashSet<TileMapMember>();
        }

        public void RegisterInTileMap(TileMapMember member)
        {
            members.Add(member);
        }
        public void DeRegisterInTileMap(TileMapMember member)
        {
            members.Remove(member);
        }
    }

    public class CoordinateSystemMembers<T> : CoordinateSystemMembersAllCoordinates, ISaveable<TileMembersSaveObject<T>> where T : ICoordinate
    {
        public ICoordinateSystem<T> coordinateSystem;
        public Action<T, TileTypeInfo> OnTileChanged;
        public TileDefinitions tileDefinitions;

        private IDictionary<T, TileTypeInfo> tiles;


        public CoordinateSystemMembers() : base()
        {
            tiles = new Dictionary<T, TileTypeInfo>();
        }

        public void SetTile(T coordinate, TileTypeInfo tileID)
        {
            if (tiles.TryGetValue(coordinate, out var currentID) && currentID.Equals(tileID))
            {
                return;
            }
            tiles[coordinate] = tileID;
            OnTileChanged?.Invoke(coordinate, tileID);
        }

        public TileProperties TilePropertiesAt(T coordinate)
        {
            var tileType = GetTileType(coordinate);
            return tileDefinitions.GetTileProperties(tileType);
        }

        public TileTypeInfo GetTileType(T coordinate)
        {
            if (tiles.TryGetValue(coordinate, out var value))
            {
                return value;
            }
            return defaultTile;
        }

        public TileMembersSaveObject<T> GetSaveObject()
        {
            return new TileMembersSaveObject<T>
            {
                tiles = tiles.Select(pair => new TileMapDataTile<T>
                {
                    coordinate = pair.Key,
                    tileType = pair.Value
                }).ToList(),
                members = allMembers.Select(member => new TileMemberSaveObject<T>
                {
                    coordinate = (T)member.CoordinatePosition,
                    objectData = member.GetSaveObject()
                }).ToList(),
                defaultTile = defaultTile
            };
        }

        public void SetupFromSaveObject(TileMembersSaveObject<T> save)
        {
            defaultTile = save.defaultTile;

            tiles = save.tiles.ToDictionary(tile => tile.coordinate, tile => tile.tileType);

            foreach (var memberData in save.members)
            {
                var instantiated = memberPrefabRegistry.GetPrefabForType(memberData.objectData.memberType, transform);
                instantiated.SetPosition(memberData.coordinate, coordinateSystem);
                instantiated.SetupFromSaveObject(memberData.objectData);
            }
        }
    }
}
