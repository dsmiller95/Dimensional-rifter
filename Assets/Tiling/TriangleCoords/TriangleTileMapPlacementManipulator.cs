using Assets.Tiling.Tilemapping;
using Assets.UI.Manipulators;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Tiling.TriangleCoords
{
    [CreateAssetMenu(fileName = "TriangleTileMapPlacement", menuName = "TileMap/Manipulators/TriangleTileMapPlacement", order = 2)]
    public class TriangleTileMapPlacementManipulator : MapManipulator
    {
        public GameObject placingPreviewPrefab;
        public RectTransform confirmUIPrefab;
        public float zLayer;

        private ManipulatorController controller;
        
        public override void OnOpen(ManipulatorController controller)
        {
            Debug.Log("opening triangle placement manipulator");
            this.controller = controller;
            //activeBuildPreview = GameObject.Instantiate(buildPreviewPrefab, controller.transform);
            regionRootCoordinate = default;
        }

        public override void OnClose()
        {
            AbortPreview();
        }


        private bool isDragging;
        private bool previewActive;
        private UniversalCoordinate regionRootCoordinate;
        private Vector2 mouseDragOrigin;

        //private GameObject placingPreview;

        public override void OnUpdate()
        {
            //UpdatePreviewPositionAndBlocking();
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                isDragging = true;
                previewActive = true;

                mouseDragOrigin = MyUtilities.GetMousePos2D();
                var originCoordinate = TriangleCoordinate.AtOrigin();

                var previewRegion = UniversalCoordinateRange.From(
                    TriangleTriangleCoordinateRange.From(originCoordinate, 1)
                    );

                var planeID = CombinationTileMapManager.instance.BeginPreviewRegion(
                    Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, zLayer)),
                    previewRegion
                    );
                regionRootCoordinate = UniversalCoordinate.From(originCoordinate, planeID);
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                StopDrag();
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                var currentPos = MyUtilities.GetMousePos2D();
                var diff = mouseDragOrigin - currentPos;
                var distance = diff.magnitude;
                var angle = Quaternion.AngleAxis(
                    Mathf.Rad2Deg * Mathf.Atan2(diff.y, diff.x) + 90,
                    Vector3.forward);

                var triangleRadius = .5f / TriangleCoordinate.vBasis.y;
                var triangleNum = Mathf.FloorToInt(distance / triangleRadius);

                var previewRegion = UniversalCoordinateRange.From(
                    TriangleTriangleCoordinateRange.From(regionRootCoordinate.triangleDataView, triangleNum)
                    );


                var transformMatrix = Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, zLayer));
                transformMatrix *= Matrix4x4.Rotate(angle);
                var transformToCenterOfTriangle = -(TriangleCoordinate.rBasis * (triangleNum * 2 - 3));
                transformMatrix *= Matrix4x4.Translate(new Vector3(transformToCenterOfTriangle.x, transformToCenterOfTriangle.y, 0));

                CombinationTileMapManager.instance.SetPreviewRegionData(
                    transformMatrix,
                    previewRegion,
                    regionRootCoordinate.CoordinatePlaneID);
            }
        }

        private RectTransform confirmUiInstance;
        private void OpenConfirmUI()
        {
            confirmUiInstance = Instantiate(confirmUIPrefab);
            var button = confirmUiInstance.gameObject.GetComponentInChildren<Button>();
        }

        private void StopDrag()
        {
            if (isDragging)
            {
                isDragging = false;
                mouseDragOrigin = default;
            }
        }

        private void AbortPreview()
        {
            if (previewActive)
            {
                previewActive = false;
                isDragging = false;
                CombinationTileMapManager.instance.ClosePreviewRegion(regionRootCoordinate.CoordinatePlaneID);
                mouseDragOrigin = default;
                regionRootCoordinate = default;
            }
        }
    }
}
