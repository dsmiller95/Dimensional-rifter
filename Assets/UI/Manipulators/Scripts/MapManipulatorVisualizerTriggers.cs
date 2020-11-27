using Assets.UI.Manipulators.Scripts.SelectedArea;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts
{
    public class MapManipulatorVisualizerTriggers : MonoBehaviour
    {
        World world => World.DefaultGameObjectInjectionWorld;
        public void SetDragVisualizerActive(bool active)
        {
            var dragVisualizer = world.GetExistingSystem<DragSelectAreaSystem>();
            dragVisualizer.SetVisualizerEnabled(active);
        }
    }
}
