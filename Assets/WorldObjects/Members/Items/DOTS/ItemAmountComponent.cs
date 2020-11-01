using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    [GenerateAuthoringComponent]
    public struct ItemAmountComponent : IComponentData
    {
        public float maxCapacity;

        public Resource resourceType;
        public float resourceAmount;
    }
}
