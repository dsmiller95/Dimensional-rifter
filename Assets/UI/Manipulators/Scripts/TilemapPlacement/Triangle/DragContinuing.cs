using Assets.Behaviors.Scripts;
using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    public class DragContinuing : IGenericStateHandler<TriangleTileMapPlacementManipulator>
    {
        private Vector2 mouseDragOrigin;
        public DragContinuing(Vector2 mouseDragOrigin)
        {
            this.mouseDragOrigin = mouseDragOrigin;
        }

        public IGenericStateHandler<TriangleTileMapPlacementManipulator> HandleState(TriangleTileMapPlacementManipulator data)
        {
            if (!Input.GetMouseButton(0))
            {
                return new WaitForConfirm();
            }

            var currentPos = MyUtilities.GetMousePos2D();
            var triangleNum = GetTriangleSideLengthFromNextDragPosition(currentPos);
            var transformMatrix = GetTransformationBasedOnNextDragPosition(currentPos, triangleNum, data.zLayer);
            var previewRegion = UniversalCoordinateRange.From(
                TriangleTriangleCoordinateRange.From(data.regionRootCoordinate.triangleDataView, triangleNum)
                );

            CombinationTileMapManager.instance.SetPreviewRegionData(
                data.previewer,
                transformMatrix,
                previewRegion);
            return this;
        }

        private int GetTriangleSideLengthFromNextDragPosition(Vector2 dragPosition)
        {
            var distance = Vector2.Distance(mouseDragOrigin, dragPosition);

            var triangleRadius = .5f / TriangleCoordinate.vBasis.y;
            return Mathf.FloorToInt(distance / triangleRadius);
        }

        private Matrix4x4 GetTransformationBasedOnNextDragPosition(Vector2 dragPosition, int triangleSideLength, float zLayer)
        {
            var diff = mouseDragOrigin - dragPosition;
            var angle = Quaternion.AngleAxis(
                Mathf.Rad2Deg * Mathf.Atan2(diff.y, diff.x) + 90,
                Vector3.forward);

            var transformMatrix = Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, zLayer));
            transformMatrix *= Matrix4x4.Rotate(angle);
            var transformToCenterOfTriangle = -(TriangleCoordinate.rBasis * (triangleSideLength * 2 - 3));
            transformMatrix *= Matrix4x4.Translate(new Vector3(transformToCenterOfTriangle.x, transformToCenterOfTriangle.y, 0));

            return transformMatrix;
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {
        }

        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
        }
    }
}
