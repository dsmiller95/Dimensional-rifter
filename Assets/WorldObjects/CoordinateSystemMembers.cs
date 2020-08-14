using Assets.Tiling;
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

        public CoordinateSystemMembers() : base()
        {
        }
    }
}
