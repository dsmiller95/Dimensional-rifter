using Unity.Entities;

namespace Assets.UI.ThingSelection
{
    /// <summary>
    /// Indicates the component is currently selected
    ///  TODO: consider using a singleton in the <see cref="ClickToSelectedItemSystem"/> instead? less structural changes
    /// </summary>
    public struct SelectedComponentFlag : IComponentData
    {
    }
}
