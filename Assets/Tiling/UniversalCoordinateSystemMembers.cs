using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.WorldObjects;
using Assets.WorldObjects.Members;
using Assets.WorldObjects.Members.Wall;
using Assets.WorldObjects.SaveObjects;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Assets.Tiling
{

    public class UniversalCoordinateSystemMembers : MonoBehaviour, ISaveable<UniversalTileMembersSaveObject>
    {
        public TileTypeInfo defaultTile;

        public MembersRegistry memberPrefabRegistry;

        public TileDefinitions tileDefinitions;

        private NativeHashMap<UniversalCoordinate, int> tileTypes;
        private TileTypeInfo[] infoByIndex;

        private IDictionary<UniversalCoordinate, IList<TileMapMember>> tileMembers;
        public IEnumerable<TileMapMember> allMembers => tileMembers.SelectMany(pair => pair.Value);

        public UniversalCoordinateSystemMembers()
        {
            tileMembers = new Dictionary<UniversalCoordinate, IList<TileMapMember>>();
        }

        private void OnDestroy()
        {
            tileTypes.Dispose();
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

        public IEnumerable<(UniversalCoordinate, IEnumerable<TileMapMember>)> GetMembersByCoordinate()
        {
            return tileMembers.Select(x => (x.Key, x.Value.AsEnumerable()));
        }

        public void SetTile(UniversalCoordinate coordinate, TileTypeInfo tileID)
        {
            if (tileTypes.TryGetValue(coordinate, out var currentID) && currentID.Equals(tileID))
            {
                return;
            }
            int index;
            for (index = 0; index < infoByIndex.Length; index++)
            {
                if (infoByIndex[index].Equals(tileID))
                {
                    break;
                }
            }
            if (index == infoByIndex.Length)
            {
                infoByIndex = infoByIndex.Append(tileID).ToArray();
            }
            tileTypes[coordinate] = index;
        }

        public TileProperties TilePropertiesAt(UniversalCoordinate coordinate)
        {
            var tileType = GetTileType(coordinate);
            return tileDefinitions.GetTileProperties(tileType);
        }


        public TileTypeInfo GetTileType(UniversalCoordinate coordinate)
        {
            if (tileTypes.TryGetValue(coordinate, out var value))
            {
                return infoByIndex[value];
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
            using (var keyValues = tileTypes.GetKeyValueArrays(Allocator.TempJob))
            {
                return new UniversalTileMembersSaveObject
                {
                    tileKeys = keyValues.Keys.ToArray(),
                    tileValues = keyValues.Values.ToArray(),
                    tileTypeInfoByIndex = infoByIndex,
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
        }

        public NativeHashMap<UniversalCoordinate, int> GetTileTypesByCoordinateReadonlyCollection()
        {
            return tileTypes;
        }

        public TileProperties[] GetTileInfoByTypeIndex()
        {
            return infoByIndex.Select(x => tileDefinitions.GetTileProperties(x)).ToArray();
        }

        public void SetupFromSaveObject(UniversalTileMembersSaveObject save)
        {
            defaultTile = save.defaultTile;

            tileTypes = new NativeHashMap<UniversalCoordinate, int>(save.tileKeys.Length, Allocator.Persistent);
            for (int i = 0; i < save.tileKeys.Length; i++)
            {
                tileTypes[save.tileKeys[i]] = save.tileValues[i];
            }
            infoByIndex = save.tileTypeInfoByIndex;

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
