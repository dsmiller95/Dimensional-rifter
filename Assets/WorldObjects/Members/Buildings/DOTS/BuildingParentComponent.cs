using Unity.Entities;

namespace Assets.WorldObjects.Members.Items.DOTS
{
    /// <summary>
    /// add to an entity which is a building. points to another entity which stores data about the building itself
    /// </summary>
    public struct BuildingParentComponent : IComponentData
    {
        /// <summary>
        /// building entity should have these components, at least: 
        ///     coordinate position
        ///     ItemAmountsDataComponent
        ///     ItemAmountClaimBufferData buffer
        ///     SupplyTypeComponent
        /// </summary>
        public Entity buildingEntity;
    }
}
