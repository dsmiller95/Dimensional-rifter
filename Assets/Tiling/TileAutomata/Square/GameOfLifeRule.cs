using Assets.Tiling.SquareCoords;
using Assets.WorldObjects;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.TileAutomata.Square
{
    [CreateAssetMenu(fileName = "GameOfLifeRule", menuName = "AutomataRules/Square/GameOfLife", order = 10)]
    public class GameOfLifeRule : AutomataRule<SquareCoordinate>
    {
        public string targetBaseType = "ground";
        public TileTypeInfo replaceType = new TileTypeInfo("ground", "NO_BORDERS");

        public override bool TryMatch(SquareCoordinate coordinate, CoordinateSystemMembers<SquareCoordinate> members)
        {
            var me = members.GetTileType(coordinate);
            if(me.baseID == targetBaseType)
            {
                return false;
            }
            var neighbors = members.coordinateSystem.Neighbors(coordinate).Select(x => members.GetTileType(x));

            var numberOfSame = neighbors.Where(x => x.baseID == targetBaseType).Count();
            if (numberOfSame >= 3)
            {
                members.SetTile(coordinate, replaceType);
                return true;
            }
            return false;
        }
    }
}
