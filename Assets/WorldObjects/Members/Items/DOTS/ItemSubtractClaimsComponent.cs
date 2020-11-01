using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    [GenerateAuthoringComponent]
    public struct ItemSubtractClaimComponent : IComponentData
    {
        public float TotalAllocatedSubtractions;
    }
}
