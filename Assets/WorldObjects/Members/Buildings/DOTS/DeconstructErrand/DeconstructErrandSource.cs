using Assets.Scripts.DOTS.ErrandClaims;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.DeconstructErrand
{
    public class DeconstructErrandSource :
        BasicErrandSource<DeconstructErrand, DeconstructErrandRequestComponent, DeconstructErrandResultComponent>
    {
        protected override DeconstructErrandRequestComponent GenerateRequestComponent(GameObject errandExecutor)
        {
            return new DeconstructErrandRequestComponent
            {
                DataIsSet = true,
            };
        }

        protected override DeconstructErrand GenerateErrandFromResponse(
            DeconstructErrandResultComponent response,
            GameObject errandExecutor)
        {
            return new DeconstructErrand(response, World.DefaultGameObjectInjectionWorld, errandExecutor, this);
        }
    }
}
