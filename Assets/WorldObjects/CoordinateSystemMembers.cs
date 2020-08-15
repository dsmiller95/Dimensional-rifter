using Assets.Tiling;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WorldObjects
{

    public class CoordinateSytemContainer
    {
        public ICoordinateSystem system;
        public bool isCompatible(ICoordinate coordinate)
        {
            return system.IsCompatible(coordinate);
        }
    }

    [Serializable]
    public struct TileTypeInfo
    {
        public TileTypeInfo(string baseId, string shape) {
            this.baseID = baseId;
            this.shapeID = shape;
        }
        public string ID => baseID + shapeID;
        public string baseID;
        public string shapeID;
        public override bool Equals(object obj)
        {
            if(obj is TileTypeInfo other)
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

    public class CoordinateSystemMembers<T> : CoordinateSystemMembersAllCoordinates where T : ICoordinate
    {
        public ICoordinateSystem<T> coordinateSystem;
        public Action<T, TileTypeInfo> OnTileChanged;

        private IDictionary<T, TileTypeInfo> tiles;

        public CoordinateSystemMembers() : base()
        {
            tiles = new Dictionary<T, TileTypeInfo>();
        }

        public void SetTile(T coordinate, TileTypeInfo tileID)
        {
            if(tiles.TryGetValue(coordinate, out var currentID) && currentID.Equals(tileID))
            {
                return;
            }
            tiles[coordinate] = tileID;
            this.OnTileChanged?.Invoke(coordinate, tileID);
        }

        public TileTypeInfo GetTileType(T coordinate)
        {
            if(tiles.TryGetValue(coordinate, out var value))
            {
                return value;
            }
            return defaultTile;
        }

    }
}
