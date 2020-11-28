using Unity.Entities;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    public struct DeconstructBuildingClaimComponent : IComponentData
    {
        public bool DeconstructClaimed;
    }
}
