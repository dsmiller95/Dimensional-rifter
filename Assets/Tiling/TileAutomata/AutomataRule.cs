using UnityEngine;

namespace Assets.Tiling.TileAutomata
{
    public abstract class AutomataRule : ScriptableObject
    {
        public abstract bool TryMatch(UniversalCoordinate coordinate, UniversalCoordinateSystemMembers members);
    }
}
