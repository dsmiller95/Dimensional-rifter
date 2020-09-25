using Assets.Tiling.Tilemapping;
using UnityEngine;

namespace Assets.Tiling.TileAutomata
{
    [CreateAssetMenu(fileName = "AutomataSystem", menuName = "MapGeneration/Automata/AutomataSystem", order = 1)]
    public class AutomataSystem : ScriptableObject
    {
        public AutomataRule[] rules;

        public void ExecuteOnRegion(TheReelBigCombinationTileMapManager manager, short layerID)
        {
            var regionData = manager.allRegions[layerID];
            ExecuteAutomataStep(regionData.baseRange, manager.everyMember);
        }

        public void ExecuteAutomataStep(IUniversalCoordinateRange coordinates, UniversalCoordinateSystemMembers tileMemebers)
        {
            foreach (var coordinate in coordinates.GetUniversalCoordinates())
            {
                foreach (var rule in rules)
                {
                    if (rule.TryMatch(coordinate, tileMemebers))
                    {
                        break;
                    }
                }
            }
        }
    }
}
