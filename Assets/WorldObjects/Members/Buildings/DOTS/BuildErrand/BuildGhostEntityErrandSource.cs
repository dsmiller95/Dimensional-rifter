using Assets.Scripts.DOTS.ErrandClaims;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS.BuildErrand
{
    public class BuildGhostEntityErrandSource :
        BasicErrandSource<BuildEntityErrand, BuildErrandRequestComponent, BuildErrandResultComponent>
    {
        protected override BuildErrandRequestComponent GenerateRequestComponent(GameObject errandExecutor)
        {
            return new BuildErrandRequestComponent
            {
                DataIsSet = true,
            };
        }

        protected override BuildEntityErrand GenerateErrandFromResponse(
            BuildErrandResultComponent response,
            GameObject errandExecutor)
        {
            return new BuildEntityErrand(response, World.DefaultGameObjectInjectionWorld, errandExecutor, this);
        }
    }
}
