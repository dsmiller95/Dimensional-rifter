using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.WorldObjects;
using Assets.WorldObjects.Members;
using Assets.WorldObjects.Members.Wall;
using Assets.WorldObjects.SaveObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling
{

    public class UniversalCoordinateSystemMembers : MonoBehaviour, ISaveable<UniversalTileMembersSaveObject>
    {
        public TileTypeInfo defaultTile;

        public MembersRegistry memberPrefabRegistry;

        public Action<UniversalCoordinate, TileTypeInfo> OnTileChanged;
        public TileDefinitions tileDefinitions;

        private IDictionary<UniversalCoordinate, TileTypeInfo> tiles;
        private IDictionary<UniversalCoordinate, IList<TileMapMember>> tileMembers;
        public IEnumerable<TileMapMember> allMembers => tileMembers.SelectMany(pair => pair.Value);

        public UniversalCoordinateSystemMembers()
        {
            tiles = new Dictionary<UniversalCoordinate, TileTypeInfo>();
            tileMembers = new Dictionary<UniversalCoordinate, IList<TileMapMember>>();
        }
        public void RegisterInTileMap(TileMapMember member)
        {
            var coord = member.CoordinatePosition;
            if (tileMembers.TryGetValue(coord, out var list))
            {
                list.Add(member);
            }
            else
            {
                var newList = new List<TileMapMember>
                {
                    member
                };
                tileMembers[coord] = newList;
            }
        }
        public void DeRegisterInTileMap(TileMapMember member)
        {
            var coord = member.CoordinatePosition;
            if (tileMembers.TryGetValue(coord, out var list))
            {
                list.Remove(member);
            }
        }


        public void SetTile(UniversalCoordinate coordinate, TileTypeInfo tileID)
        {
            if (tiles.TryGetValue(coordinate, out var currentID) && currentID.Equals(tileID))
            {
                return;
            }
            tiles[coordinate] = tileID;
            OnTileChanged?.Invoke(coordinate, tileID);
        }

        public TileProperties TilePropertiesAt(UniversalCoordinate coordinate)
        {
            var tileType = GetTileType(coordinate);
            return tileDefinitions.GetTileProperties(tileType);
        }


        public TileTypeInfo GetTileType(UniversalCoordinate coordinate)
        {
            if (tiles.TryGetValue(coordinate, out var value))
            {
                return value;
            }
            return defaultTile;
        }
        public IList<TileMapMember> GetMembersOnTile(UniversalCoordinate coordinate)
        {
            if (tileMembers.TryGetValue(coordinate, out var value))
            {
                return value;
            }
            return new TileMapMember[0];
        }
        public bool IsPassable(UniversalCoordinate coordinate)
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

        public UniversalTileMembersSaveObject GetSaveObject()
        {
            return new UniversalTileMembersSaveObject
            {
                tiles = tiles.Select(pair => new TileMapDataTile
                {
                    coordinate = pair.Key,
                    tileType = pair.Value
                }).ToList(),
                members = allMembers
                    .Where(member => member.memberType != null)
                    .Select(member => new TileMemberSaveObject
                    {
                        coordinate = member.CoordinatePosition,
                        objectData = member.GetSaveObject()
                    }).ToList(),
                defaultTile = defaultTile
            };
        }

        public void SetupFromSaveObject(UniversalTileMembersSaveObject save)
        {
            defaultTile = save.defaultTile;

            tiles = save.tiles.ToDictionary(tile => tile.coordinate, tile => tile.tileType);

            foreach (var memberData in save.members)
            {
                var newType = memberPrefabRegistry.GetUniqueObjectFromID(memberData.objectData.memberID);

                var instantiated = Instantiate(newType.memberPrefab, transform).GetComponent<TileMapMember>();
                instantiated?.SetPosition(memberData.coordinate);
                instantiated?.SetupFromSaveObject(memberData.objectData);
            }
        }
    }
}
