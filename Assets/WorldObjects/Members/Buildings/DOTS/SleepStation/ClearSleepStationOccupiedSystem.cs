using Assets.WorldObjects.SaveObjects.SaveManager;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    [UpdateInGroup(typeof(PostDeserialzeSystemGroup))]
    public class ClearSleepStationOccupiedSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref SleepStationOccupiedComponent occupied) =>
            {
                occupied.Occupied = false;
            }).ScheduleParallel();
        }
    }
}
