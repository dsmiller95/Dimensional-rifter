using Assets.Behaviors.Scripts;
using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    internal class DragStartDetectState : IGenericStateHandler<TriangleTileMapPlacementManipulator>
    {
        public IGenericStateHandler<TriangleTileMapPlacementManipulator> HandleState(TriangleTileMapPlacementManipulator data)
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return this;
            }
            var mouseDragOrigin = MyUtilities.GetMousePos2D();
            var originCoordinate = TriangleCoordinate.AtOrigin();

            var previewRegion = UniversalCoordinateRange.From(
                TriangleTriangleCoordinateRange.From(originCoordinate, 1)
                );

            var planeID = CombinationTileMapManager.instance.BeginPreviewRegion(
                Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, data.zLayer)),
                previewRegion
                );
            var regionRootCoordinate = UniversalCoordinate.From(originCoordinate, planeID);
            data.regionRootCoordinate = regionRootCoordinate;
            return new DragContinuing(mouseDragOrigin);
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {
            data.regionRootCoordinate = default;
        }

        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
        }
    }
}
