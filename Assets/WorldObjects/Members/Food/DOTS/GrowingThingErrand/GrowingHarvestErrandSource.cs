using Assets.Scripts.DOTS.ErrandClaims;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food.DOTS.GrowingThingErrand
{
    public class GrowingHarvestErrandSource :
        BasicErrandSource<GrowingHarvestErrand, GrowingHarvestErrandRequestComponent, GrowingHarvestErrandResultComponent>
    {
        protected override GrowingHarvestErrandRequestComponent GenerateRequestComponent(GameObject errandExecutor)
        {
            return new GrowingHarvestErrandRequestComponent
            {
                DataIsSet = true,
            };
        }

        protected override GrowingHarvestErrand GenerateErrandFromResponse(
            GrowingHarvestErrandResultComponent response,
            GameObject errandExecutor)
        {
            return new GrowingHarvestErrand(response, World.DefaultGameObjectInjectionWorld, errandExecutor, this);
        }
    }
}
