using Assets.Tiling.Tilemapping.NEwSHITE;
using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.TileAutomata.Square
{
    [Serializable]
    public struct TriangleTileNeighborFlags
    {
        public bool Self;
        public bool First;
        public bool Second;
        public bool Third;
        public TriangleTileShapes Shape;
        public override bool Equals(object obj)
        {
            if (obj is TriangleTileNeighborFlags flags)
            {
                return flags.Self == Self &&
                    flags.First == First &&
                    flags.Second == Second &&
                    flags.Third == Third;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (First ? 1 : 0) |
                (Second ? 2 : 0) |
                (Third ? 4 : 0) |
                (Self ? 8 : 0);
        }
    }

    [CreateAssetMenu(fileName = "TriangeShapeRule", menuName = "MapGeneration/Automata/Triangle/TileShape", order = 10)]
    public class TriangleShapeRule : AutomataRule
    {
        public string targetBaseType = "ground";

        public TriangleTileNeighborFlags[] validMatches;

        public override bool TryMatch(UniversalCoordinate coordinate, UniversalCoordinateSystemMembers members)
        {
            if (coordinate.type != CoordinateType.TRIANGLE)
            {
                return false;
            }
            var leftover = validMatches
                .Where(match => match.Self == GetFlag(coordinate, members));

            var neighbors = coordinate.Neighbors().ToArray();

            leftover = leftover
                .Where(match => match.First == GetFlag(neighbors[0], members))
                .Where(match => match.Second == GetFlag(neighbors[1], members))
                .Where(match => match.Third == GetFlag(neighbors[2], members));

            var matched = leftover.Cast<TriangleTileNeighborFlags?>().FirstOrDefault();
            if (matched.HasValue)
            {
                var tileInfo = new TileTypeInfo(targetBaseType, Enum.GetName(typeof(TriangleTileShapes), matched.Value.Shape));
                members.SetTile(coordinate, tileInfo);
                return true;
            }
            return false;
        }

        private bool GetFlag(
            UniversalCoordinate coordinate,
            UniversalCoordinateSystemMembers members)
        {
            return members.GetTileType(coordinate).baseID == targetBaseType;
        }
    }
}
