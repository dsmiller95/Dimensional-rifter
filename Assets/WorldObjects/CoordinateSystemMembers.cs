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

    public class CoordinateSystemMembersAllCoordinates : MonoBehaviour
    {
        public IEnumerable<TileMapMember> allMembers => members;
        public string defaultTile;


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
        private IDictionary<T, string> tiles;
        public Action<T, string> OnTileSet;

        public CoordinateSystemMembers() : base()
        {
            tiles = new Dictionary<T, string>();
        }
        public void SetTile(T coordinate, string tileID)
        {
            tiles[coordinate] = tileID;
            this.OnTileSet?.Invoke(coordinate, tileID);
        }

        public string GetTileType(T coordinate)
        {
            if(tiles.TryGetValue(coordinate, out var value))
            {
                return value;
            }
            return defaultTile;
        }

    }
}
