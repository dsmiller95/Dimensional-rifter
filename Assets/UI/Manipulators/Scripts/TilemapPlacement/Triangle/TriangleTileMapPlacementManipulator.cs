using Assets.Behaviors.Scripts;
using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    [CreateAssetMenu(fileName = "TriangleTileMapPlacement", menuName = "TileMap/Manipulators/TriangleTileMapPlacement", order = 2)]
    public class TriangleTileMapPlacementManipulator : MapManipulator
    {
        public GameObject placingPreviewPrefab;
        public float zLayer;

        private ManipulatorController controller;

        private StateMachine<TriangleTileMapPlacementManipulator> dragEditStateMachine;

        public UniversalCoordinate regionRootCoordinate;
        public TileMapRegionPreview previewer;

        public override void OnOpen(ManipulatorController controller)
        {
            this.controller = controller;
            dragEditStateMachine = new StateMachine<TriangleTileMapPlacementManipulator>(new DragStartDetectState());
        }

        public override void OnClose()
        {
            dragEditStateMachine.ForceSetState(new DragStartDetectState(), this);
            dragEditStateMachine = null;
        }

        public override void OnUpdate()
        {
            dragEditStateMachine.update(this);
        }
    }
}
