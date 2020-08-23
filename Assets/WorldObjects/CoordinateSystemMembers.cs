using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.Members;
using Assets.WorldObjects.Members.Wall;
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

    public abstract class CoordinateSystemMembersAllCoordinates : MonoBehaviour
    {
        public abstract IEnumerable<TileMapMember> allMembers { get; }
        public TileTypeInfo defaultTile;

        public MembersRegistry memberPrefabRegistry;

        public CoordinateSystemMembersAllCoordinates()
        {
        }

        public abstract void RegisterInTileMap(TileMapMember member);
        public abstract void DeRegisterInTileMap(TileMapMember member);

        public abstract bool IsPassableTypeUnsafe(ICoordinate coord);
    }

    public class CoordinateSystemMembers<T> : CoordinateSystemMembersAllCoordinates, ISaveable<TileMembersSaveObject<T>> where T : ICoordinate
    {
        public ICoordinateSystem<T> coordinateSystem;
        public Action<T, TileTypeInfo> OnTileChanged;
        public TileDefinitions tileDefinitions;

        private IDictionary<T, TileTypeInfo> tiles;
        private IDictionary<T, IList<TileMapMember>> tileMembers;
        public override IEnumerable<TileMapMember> allMembers => tileMembers.SelectMany(pair => pair.Value);

        public CoordinateSystemMembers() : base()
        {
            tiles = new Dictionary<T, TileTypeInfo>();
            tileMembers = new Dictionary<T, IList<TileMapMember>>();
        }
        public override void RegisterInTileMap(TileMapMember member)
        {
            if (member.CoordinatePosition is T coordinate)
            {
                if (tileMembers.TryGetValue(coordinate, out var list))
                {
                    list.Add(member);
                }
                else
                {
                    var newList = new List<TileMapMember>
                    {
                        member
                    };
                    tileMembers[coordinate] = newList;
                }
            }
            else
            {
                throw new Exception("Cannot register tile member: coordinate position is of incompatable type");
            }
        }
        public override void DeRegisterInTileMap(TileMapMember member)
        {
            if (member.CoordinatePosition == null)
            {
                return;
            }
            if (member.CoordinatePosition is T coordinate)
            {
                if (tileMembers.TryGetValue(coordinate, out var list))
                {
                    list.Remove(member);
                }
            }
            else
            {
                throw new Exception("Cannot register tile member: coordinate position is of incompatable type");
            }
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
        public IEnumerable<TileMapMember> GetMembersOnTile(T coordinate)
        {
            if (tileMembers.TryGetValue(coordinate, out var value))
            {
                return value;
            }
            return new TileMapMember[0];
        }
        public override bool IsPassableTypeUnsafe(ICoordinate coord)
        {
            return IsPassable((T)coord);
        }
        public bool IsPassable(T coordinate)
        {
            var props = TilePropertiesAt(coordinate);
            if (!props.isPassable)
            {
                return false;
            }
            if (GetMembersOnTile(coordinate).Any(tile => tile.GetComponent<ITileBlocking>()?.IsCurrentlyBlocking ?? false))
            {
                return false;
            }
            return true;
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
                members = allMembers
                    .Where(member => member.memberType != null)
                    .Select(member => new TileMemberSaveObject<T>
                {
                    coordinate = (T)member.CoordinatePosition,
                    objectData = member.GetSaveObject()
                }).ToList(),
                defaultTile = defaultTile
            };
        }

        public void SetupFromSaveObject(TileMembersSaveObject<T> save)
        {
            var myRegion = GetComponent<TileMapRegion<T>>();
            defaultTile = save.defaultTile;

            tiles = save.tiles.ToDictionary(tile => tile.coordinate, tile => tile.tileType);

            foreach (var memberData in save.members)
            {
                var newType = memberPrefabRegistry.GetMemberFromUniqueInfo(memberData.objectData.memberType);

                var instantiated = Instantiate(newType.memberPrefab, transform).GetComponent<TileMapMember>();
                instantiated?.SetPosition(memberData.coordinate, myRegion);
                instantiated?.SetupFromSaveObject(memberData.objectData);
            }
        }
    }
}
