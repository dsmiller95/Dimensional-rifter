using Assets.Scripts.DOTS.ErrandClaims;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food.DOTS.GrowingThingErrand
{
    public class GrowingHarvestErrandSource :
        BasicErrandSource<GrowingHarvesErrand, GrowingHarvestErrandRequestComponent, GrowingHarvestErrandResultComponent>
    {
        protected override GrowingHarvestErrandRequestComponent GenerateRequestComponent(GameObject errandExecutor)
        {
            return new GrowingHarvestErrandRequestComponent
            {
                DataIsSet = true,
            };
        }

        protected override GrowingHarvesErrand GenerateErrandFromResponse(
            GrowingHarvestErrandResultComponent response,
            GameObject errandExecutor)
        {
            return new GrowingHarvesErrand(response, World.DefaultGameObjectInjectionWorld, errandExecutor, this);
        }
    }
}
