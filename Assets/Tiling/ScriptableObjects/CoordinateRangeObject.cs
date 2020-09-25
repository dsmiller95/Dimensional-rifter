using Assets.Tiling.Tilemapping.NEwSHITE;
using UnityEngine;

namespace Assets.Tiling.ScriptableObjects
{
    public abstract class CoordinateRangeObject : ScriptableObject
    {
        public abstract IUniversalCoordinateRange CoordinateRange { get; }
    }
}
