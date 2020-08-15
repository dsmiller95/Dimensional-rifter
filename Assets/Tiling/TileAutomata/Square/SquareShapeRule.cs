using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping.TileConfiguration;
using Assets.WorldObjects;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.TileAutomata.Square
{
    [Serializable]
    public struct SquareTileNeighborFlags
    {
        public bool Self;
        public bool Top;
        public bool Right;
        public bool Bottom;
        public bool Left;
        public SqaureTileShape Shape;
        public override bool Equals(object obj)
        {
            if (obj is SquareTileNeighborFlags flags)
            {
                return flags.Self == Self &&
                    flags.Top == Top &&
                    flags.Right == Right &&
                    flags.Bottom == Bottom &&
                    flags.Left == Left;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (Top ? 1 : 0) |
                (Right ? 2 : 0) |
                (Bottom ? 4 : 0) |
                (Left ? 8 : 0) |
                (Self ? 16 : 0);
        }
    }

    [CreateAssetMenu(fileName = "SquareShapeRule", menuName = "AutomataRules/Square/TileShape", order = 1)]
    public class SquareShapeRule : AutomataRule<SquareCoordinate>
    {
        public string targetBaseType = "ground";

        public SquareTileNeighborFlags[] validMatches;

        public override bool TryMatch(SquareCoordinate coordinate, CoordinateSystemMembers<SquareCoordinate> members)
        {
            var leftover = validMatches
                .Where(match => match.Self == GetFlag(coordinate, members))
                .Where(match => match.Top == GetFlag(coordinate + SquareCoordinate.UP, members))
                .Where(match => match.Bottom == GetFlag(coordinate + SquareCoordinate.DOWN, members))
                .Where(match => match.Right == GetFlag(coordinate + SquareCoordinate.RIGHT, members))
                .Where(match => match.Left == GetFlag(coordinate + SquareCoordinate.LEFT, members));

            var matched = leftover.Cast<SquareTileNeighborFlags?>().FirstOrDefault();
            if (matched.HasValue)
            {
                var tileInfo = new TileTypeInfo(targetBaseType, Enum.GetName(typeof(SqaureTileShape), matched.Value.Shape));
                members.SetTile(coordinate, tileInfo);
                return true;
            }
            return false;
        }

        private bool GetFlag(SquareCoordinate coordinate, CoordinateSystemMembers<SquareCoordinate> members)
        {
            return members.GetTileType(coordinate).baseID == targetBaseType;
        }
    }
}
