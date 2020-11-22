using ECS_SpriteSheetAnimation.FlibookComponents;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    public class SleepStationOccupyEnableAnimationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<SleepStationOccupiedComponent>()
                .ForEach((
                    ref FlipbookAnimatorEnabledComponent flipbookEnabled,
                    in SleepStationOccupiedComponent sleepStationOccupied) =>
                {
                    flipbookEnabled.Value = sleepStationOccupied.Occupied;
                }).ScheduleParallel();
        }
    }
}