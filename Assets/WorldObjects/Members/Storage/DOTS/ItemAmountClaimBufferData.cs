using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS
{
    public struct ItemAmountClaimBufferData : IBufferElementData
    {
        public float TotalSubtractionClaims;
        public float Amount;
        public Resource Type;
        public bool HasAvailableAmount()
        {
            return (Amount - TotalSubtractionClaims) > 0;
        }
    }

    public static class ItemAmountClaimBufferMethods
    {
        public static float TotalAmounts(this DynamicBuffer<ItemAmountClaimBufferData> claimBuffer)
        {
            var totalAmounts = 0f;
            foreach (var resourceAmount in claimBuffer)
            {
                totalAmounts += resourceAmount.Amount;
            }
            return totalAmounts;
        }
    }
}
