using Assets.Scripts.DOTS.ErrandClaims;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry.DOTS.EatingErrand
{
    public class EatingErrandSource :
        BasicErrandSource<EatingEntityErrand, SpecificResourceConsumeRequestComponent, SpecificResourceErrandResultComponent>
    {
        protected override SpecificResourceConsumeRequestComponent GenerateRequestComponent(GameObject errandExecutor)
        {
            var hungryBoi = errandExecutor.GetComponent<Hungry>();

            return new SpecificResourceConsumeRequestComponent
            {
                DataIsSet = true,
                ItemSourceTypeFlags = uint.MaxValue,
                resourceToConsume = Resource.FOOD,
                maxResourceConsume = hungryBoi.MaxAmountCanBeEatenOfResource(Resource.FOOD)
            };
        }

        protected override EatingEntityErrand GenerateErrandFromResponse(
            SpecificResourceErrandResultComponent response,
            GameObject errandExecutor)
        {
            return new EatingEntityErrand(response, World.DefaultGameObjectInjectionWorld, errandExecutor, this);
        }
    }
}
