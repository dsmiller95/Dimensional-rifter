using Assets.Tiling.Tilemapping;
using Assets.UI.Manipulators;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Tiling.TriangleCoords
{
    [CreateAssetMenu(fileName = "TriangleTileMapPlacement", menuName = "TileMap/Manipulators/TriangleTileMapPlacement", order = 2)]
    public class TriangleTileMapPlacementManipulator : MapManipulator
    {
        public GameObject placingPreviewPrefab;
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
        private TriangleCoordinate regionRootCoordinate;
        private Vector2 mouseDragOrigin;
        //private GameObject placingPreview;

        public override void OnUpdate()
        {
            //UpdatePreviewPositionAndBlocking();
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                isDragging = true;

                mouseDragOrigin = MyUtilities.GetMousePos2D();
                regionRootCoordinate = TriangleCoordinate.AtOrigin();

                var previewRegion = UniversalCoordinateRange.From(
                    TriangleTriangleCoordinateRange.From(regionRootCoordinate, 1)
                    );

                CombinationTileMapManager.instance.BeginPreviewRegion(
                    Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, zLayer)),
                    previewRegion
                    );
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                AbortPreview();
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
                    TriangleTriangleCoordinateRange.From(regionRootCoordinate, triangleNum)
                    );


                var transformMatrix = Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, zLayer));
                transformMatrix *= Matrix4x4.Rotate(angle);
                var transformToCenterOfTriangle = -(TriangleCoordinate.rBasis * (triangleNum * 2 - 3));
                transformMatrix *= Matrix4x4.Translate(new Vector3(transformToCenterOfTriangle.x, transformToCenterOfTriangle.y, 0));

                CombinationTileMapManager.instance.SetPreviewRegionData(
                    transformMatrix,
                    previewRegion);
            }
        }
        private void AbortPreview()
        {
            if (isDragging)
            {
                isDragging = false;
                CombinationTileMapManager.instance.ClosePreviewRegion();
                mouseDragOrigin = default;
                regionRootCoordinate = default;
            }
        }
    }
}
