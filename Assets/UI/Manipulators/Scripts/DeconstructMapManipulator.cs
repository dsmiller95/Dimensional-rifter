using Assets.UI.Manipulators.Scripts;
using Assets.UI.Manipulators.Scripts.SelectedArea;
using Assets.WorldObjects.Members.Buildings.DOTS;
using Unity.Entities;
using UnityEngine;

namespace Assets.UI.Manipulators
{
    [CreateAssetMenu(fileName = "DeconstructMapManipulator", menuName = "TileMap/Manipulators/Deconstruct", order = 1)]
    public class DeconstructMapManipulator : MapManipulator
    {
        private AreaSelectedSystemGroup areaSelectedGroup => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<AreaSelectedSystemGroup>();
        private DeconstructSelectedAreaSystem deconstructSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DeconstructSelectedAreaSystem>();
        private DragSelectAreaVisualizerSystem dragSelectVisualizer => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DragSelectAreaVisualizerSystem>();
        public override void OnOpen(ManipulatorController controller)
        {
            Debug.Log("opening deconstruct manipulator");
            areaSelectedGroup.DisableSystemsInGroup();
            deconstructSystem.Enabled = true;
            dragSelectVisualizer.SetVisualizerEnabled(true);
        }

        public override void OnClose()
        {
            Debug.Log("closing deconstruct manipulator");
            areaSelectedGroup.DisableSystemsInGroup();
            dragSelectVisualizer.SetVisualizerEnabled(false);
        }

        public override void OnUpdate()
        {
        }
    }
}
