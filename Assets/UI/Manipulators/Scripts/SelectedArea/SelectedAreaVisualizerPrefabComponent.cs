using Unity.Entities;

namespace Assets.UI.Manipulators.Scripts.SelectedArea
{
    [GenerateAuthoringComponent]
    public struct SelectedAreaVisualizerPrefabComponent : IComponentData
    {
        public Entity Value;
    }
}
