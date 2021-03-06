﻿using Assets.Tiling.Tilemapping.TileConfiguration;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.TileAutomata.Square
{
    [CreateAssetMenu(fileName = "GameOfLifeRule", menuName = "MapGeneration/Automata/Square/GameOfLife", order = 100)]
    public class GameOfLifeRule : AutomataRule
    {
        public string targetBaseType = "ground";
        public TileTypeInfo replaceType = new TileTypeInfo("ground", "NO_BORDERS");

        public override bool TryMatch(UniversalCoordinate coordinate, UniversalCoordinateSystemMembers members)
        {
            var me = members.GetTileType(coordinate);
            if (me.baseID == targetBaseType)
            {
                return false;
            }
            var neighbors = coordinate.Neighbors().Select(x => members.GetTileType(x));

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
