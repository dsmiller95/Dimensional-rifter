using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping.NEwSHITE;
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
        public SquareTileShapes Shape;
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

    [CreateAssetMenu(fileName = "SquareShapeRule", menuName = "MapGeneration/Automata/Square/TileShape", order = 10)]
    public class SquareShapeRule : AutomataRule
    {
        public string targetBaseType = "ground";

        public SquareTileNeighborFlags[] validMatches;

        public override bool TryMatch(UniversalCoordinate coordinate, UniversalCoordinateSystemMembers members)
        {
            if(coordinate.type != CoordinateType.SQUARE)
            {
                return false;
            }
            var squareboi = coordinate.squareDataView;
            var planeID = coordinate.CoordinatePlaneID;
            var leftover = validMatches
                .Where(match => match.Self == GetFlag(squareboi, members, planeID))
                .Where(match => match.Top == GetFlag(squareboi + SquareCoordinate.UP, members, planeID))
                .Where(match => match.Bottom == GetFlag(squareboi + SquareCoordinate.DOWN, members, planeID))
                .Where(match => match.Right == GetFlag(squareboi + SquareCoordinate.RIGHT, members, planeID))
                .Where(match => match.Left == GetFlag(squareboi + SquareCoordinate.LEFT, members, planeID));

            var matched = leftover.Cast<SquareTileNeighborFlags?>().FirstOrDefault();
            if (matched.HasValue)
            {
                var tileInfo = new TileTypeInfo(targetBaseType, Enum.GetName(typeof(SquareTileShapes), matched.Value.Shape));
                members.SetTile(coordinate, tileInfo);
                return true;
            }
            return false;
        }

        private bool GetFlag(
            SquareCoordinate coordinate,
            UniversalCoordinateSystemMembers members,
            short planeID)
        {
            var universalcoord = UniversalCoordinate.From(coordinate, planeID);
            return members.GetTileType(universalcoord).baseID == targetBaseType;
        }
    }
}
