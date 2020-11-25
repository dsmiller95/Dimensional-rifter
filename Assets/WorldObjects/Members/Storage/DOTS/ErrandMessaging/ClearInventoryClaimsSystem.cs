using Assets.WorldObjects.SaveObjects.SaveManager;
using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging
{
    [UpdateInGroup(typeof(PostDeserialzeSystemGroup))]
    public class ClearInventoryClaimsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((
                ref ItemAmountsDataComponent inventoryData,
                ref DynamicBuffer<ItemAmountClaimBufferData> inventoryAmounts) =>
            {
                inventoryData.TotalAdditionClaims = 0;
                for (int i = 0; i < inventoryAmounts.Length; i++)
                {
                    var amt = inventoryAmounts[i];
                    amt.TotalSubtractionClaims = 0;
                    inventoryAmounts[i] = amt;
                }
            }).ScheduleParallel();
        }
    }
}
