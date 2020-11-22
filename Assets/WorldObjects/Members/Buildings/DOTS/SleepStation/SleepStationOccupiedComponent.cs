using Unity.Entities;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    [GenerateAuthoringComponent]
    public struct SleepStationOccupiedComponent : IComponentData
    {
        public bool Occupied;
    }
}
