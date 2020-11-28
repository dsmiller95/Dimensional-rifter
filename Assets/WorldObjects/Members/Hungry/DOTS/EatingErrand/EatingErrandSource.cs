using Assets.Behaviors.Errands.Scripts;
using Assets.Scripts.DOTS.ErrandClaims;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging;
using Assets.WorldObjects.SaveObjects.SaveManager;
using BehaviorTree.Nodes;
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
