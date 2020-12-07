using Assets.Behaviors.Scripts;
using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    [CreateAssetMenu(fileName = "TriangleTileMapPlacement", menuName = "TileMap/Manipulators/TriangleTileMapPlacement", order = 2)]
    public class TriangleTileMapPlacementManipulator : MapManipulator
    {
        public GameObject placingPreviewPrefab;
        public RectTransform confirmUIPrefab;
        public float zLayer;

        private ManipulatorController controller;

        private StateMachine<TriangleTileMapPlacementManipulator> dragEditStateMachine;

        public override void OnOpen(ManipulatorController controller)
        {
            Debug.Log("opening triangle placement manipulator");
            this.controller = controller;
            //activeBuildPreview = GameObject.Instantiate(buildPreviewPrefab, controller.transform);
            regionRootCoordinate = default;
            dragEditStateMachine = new StateMachine<TriangleTileMapPlacementManipulator>(new DragStartDetectState());
        }

        public override void OnClose()
        {
            dragEditStateMachine.ForceSetState(new DragStartDetectState(), this);
            dragEditStateMachine = null;
        }


        private bool isDragging;
        private UniversalCoordinate regionRootCoordinate;
        private Vector2 mouseDragOrigin;

        //private GameObject placingPreview;

        public override void OnUpdate()
        {
            dragEditStateMachine.update(this);
        }

        private RectTransform confirmUiInstance;
        private void OpenConfirmUI()
        {
            confirmUiInstance = Instantiate(confirmUIPrefab);
            var button = confirmUiInstance.gameObject.GetComponentInChildren<Button>();
        }

        private void AbortPreview()
        {
            isDragging = false;
            CombinationTileMapManager.instance.ClosePreviewRegion(regionRootCoordinate.CoordinatePlaneID);
            mouseDragOrigin = default;
            regionRootCoordinate = default;
        }
    }
}
