using Unity.Entities;

namespace Assets.UI.ThingSelection
{
    /// <summary>
    /// A flag indicating that this entity can be selected, and can have the SelectedComponent added to it when selected
    /// </summary>
    [GenerateAuthoringComponent]
    public struct SelectableFlagComponent : IComponentData
    {
    }
}
