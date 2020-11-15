using Unity.Entities;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    [GenerateAuthoringComponent]
    public struct GrowthProductComponent : IComponentData
    {
        public Resource grownResource;
        public float resourceAmount;
    }
}
