using UnityEngine;

namespace Assets.Tiling.ScriptableObjects
{
    public abstract class CoordinateRangeObject<T> : ScriptableObject where T : ICoordinate
    {
        public abstract ICoordinateRange<T> CoordinateRange { get; }
    }
}
