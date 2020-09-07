using Assets.WorldObjects;
using UnityEngine;

namespace Assets.Tiling.TileAutomata
{
    public abstract class AutomataRule<T> : ScriptableObject where T : ICoordinate
    {
        public abstract bool TryMatch(T coordinate, CoordinateSystemMembers<T> members);
    }
}
