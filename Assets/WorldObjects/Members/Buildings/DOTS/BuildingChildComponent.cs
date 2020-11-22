using Unity.Entities;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    /// <summary>
    /// add to an entity which is a building. points to another entity which stores data about the building itself
    /// </summary>
    public struct BuildingChildComponent : IComponentData
    {
        /// <summary>
        /// pointer to the parent entity, which owns this building entity
        /// </summary>
        public Entity controllerComponent;
    }
}
