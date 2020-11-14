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
            for (var i = 0; i < claimBuffer.Length; i++)
            {
                var resourceAmount = claimBuffer[i];
                totalAmounts += resourceAmount.Amount;
            }
            return totalAmounts;
        }

        public static int IndexOfType(this DynamicBuffer<ItemAmountClaimBufferData> claimBuffer, Resource type)
        {
            for (var i = 0; i < claimBuffer.Length; i++)
            {
                var resourceAmount = claimBuffer[i];
                if (resourceAmount.Type == type)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
