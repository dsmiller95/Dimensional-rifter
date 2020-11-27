using Assets.Tiling;
using Assets.WorldObjects.DOTSMembers;
using Unity.Entities;

namespace Assets.UI.ThingSelection.ClickSelector
{
    /// <summary>
    /// origin point of drag is in <see cref="UniversalCoordinatePositionComponent"/>
    /// </summary>
    public struct DragEventComponent : IComponentData
    {
        public UniversalCoordinate dragPos;
    }

    public struct DragEventCompleteFlagComponent : IComponentData
    {
    }
}
