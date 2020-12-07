using Assets.Behaviors.Scripts;
using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    public class DragContinuing : IGenericStateHandler<TriangleTileMapPlacementManipulator>
    {
        private UniversalCoordinate regionRootCoordinate;
        private Vector2 mouseDragOrigin;
        public DragContinuing(Vector2 mouseDragOrigin, UniversalCoordinate regionRoot)
        {
            this.mouseDragOrigin = mouseDragOrigin;
            regionRootCoordinate = regionRoot;
        }

        public IGenericStateHandler<TriangleTileMapPlacementManipulator> HandleState(TriangleTileMapPlacementManipulator data)
        {
            if (!Input.GetMouseButton(0))
            {
                return new DragEnd();
            }
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


            var transformMatrix = Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, data.zLayer));
            transformMatrix *= Matrix4x4.Rotate(angle);
            var transformToCenterOfTriangle = -(TriangleCoordinate.rBasis * (triangleNum * 2 - 3));
            transformMatrix *= Matrix4x4.Translate(new Vector3(transformToCenterOfTriangle.x, transformToCenterOfTriangle.y, 0));

            CombinationTileMapManager.instance.SetPreviewRegionData(
                transformMatrix,
                previewRegion,
                regionRootCoordinate.CoordinatePlaneID);
            return this;
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {
        }

        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
        }
    }
}
