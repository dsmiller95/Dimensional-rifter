using Assets.Tiling;
using Unity.Entities;

namespace Assets.UI.Manipulators.Scripts.SelectedArea
{
    public class SelectedRangeComponent : IComponentData
    {
        public UniversalCoordinateRange selectedRange;
    }
}
