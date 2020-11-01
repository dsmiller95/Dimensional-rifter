using Unity.Entities;

namespace Assets.WorldObjects.Members.Storage.DOTS
{
    public struct ItemAmountClaimBufferData : IBufferElementData
    {
        public float TotalSubtractionClaims;
        public float Amount;
        public Resource Type;
    }
}
