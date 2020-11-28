using Assets.Scripts.DOTS.ErrandClaims;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    public class SleepStationOccupyErrandSource :
        BasicErrandSource<SleepStationOccupyErrand, SleepStationOccupyRequestComponent, SleepStationOccupyErrandResultComponent>
    {
        protected override SleepStationOccupyRequestComponent GenerateRequestComponent(GameObject errandExecutor)
        {
            return new SleepStationOccupyRequestComponent
            {
                DataIsSet = true,
            };
        }

        protected override SleepStationOccupyErrand GenerateErrandFromResponse(
            SleepStationOccupyErrandResultComponent response,
            GameObject errandExecutor)
        {
            return new SleepStationOccupyErrand(
                response,
                World.DefaultGameObjectInjectionWorld,
                errandExecutor,
                this);
        }
    }
}
