using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
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

    public class CoordinateSystemMembersAllCoordinates: MonoBehaviour
    {
        public IEnumerable<TileMapMemeber> allMembers => members;

        protected ISet<TileMapMemeber> members;
        public CoordinateSystemMembersAllCoordinates()
        {
            members = new HashSet<TileMapMemeber>();
        }

        public void RegisterInTileMap(TileMapMemeber member)
        {
            this.members.Add(member);
        }
        public void DeRegisterInTileMap(TileMapMemeber member)
        {
            this.members.Remove(member);
        }
    }

    public class CoordinateSystemMembers<T>: CoordinateSystemMembersAllCoordinates where T : ICoordinate
    {
        public ICoordinateSystem<T> coordinateSystem;

        public CoordinateSystemMembers(): base()
        {
        }
    }
}
