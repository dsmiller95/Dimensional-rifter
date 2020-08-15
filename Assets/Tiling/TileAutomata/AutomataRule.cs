using Assets.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Tiling.TileAutomata
{
    public abstract class AutomataRule<T>: ScriptableObject where T : ICoordinate
    {
        public abstract bool TryMatch(T coordinate, CoordinateSystemMembers<T> members);
    }
}
