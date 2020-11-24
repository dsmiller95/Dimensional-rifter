using Unity.Entities;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    public struct GrowthProductComponent : IComponentData
    {
        public Resource grownResource;
        public float resourceAmount;
    }
}
