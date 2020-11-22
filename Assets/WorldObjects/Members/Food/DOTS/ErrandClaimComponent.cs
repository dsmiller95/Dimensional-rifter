using Unity.Entities;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    /// <summary>
    /// Used to indicate the errand is already claimed on this entity.
    ///     use for entities that can have an atomic errand
    /// </summary>
    [GenerateAuthoringComponent]
    public struct ErrandClaimComponent : IComponentData
    {
        public bool Claimed;
    }
}
