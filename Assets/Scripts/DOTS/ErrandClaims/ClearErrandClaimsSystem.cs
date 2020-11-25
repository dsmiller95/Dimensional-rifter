using Assets.WorldObjects.SaveObjects.SaveManager;
using Unity.Entities;

namespace Assets.Scripts.DOTS.ErrandClaims
{
    [UpdateInGroup(typeof(PostDeserialzeSystemGroup))]
    public class ClearErrandClaimsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref ErrandClaimComponent errandClaim) =>
            {
                errandClaim.Claimed = false;
            }).ScheduleParallel();
        }
    }
}
