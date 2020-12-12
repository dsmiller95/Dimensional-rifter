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
            var initialData = new TileMapRegionData
            {
                coordinateTransform = Matrix4x4.Translate(new Vector3(mouseDragOrigin.x, mouseDragOrigin.y, data.zLayer)),
                planeID = -1,
                baseRange = previewRegion,
                preview = true
            };

            data.previewer = CombinationTileMapManager.instance.SpawnNewPreviewBehavior(initialData);
            data.regionRootCoordinate = UniversalCoordinate.From(originCoordinate, -1);

            data.anchorPreviewers = new System.Collections.Generic.List<WorldObjects.TileMapMember>(3);
            for (int i = 0; i < 3; i++)
            {
                data.anchorPreviewers.Add(GameObject.Instantiate(data.anchorMemberBuildingPreviewPrefab, CombinationTileMapManager.instance.transform));
            }
            data.PositionAnchors();

            return new DragContinuing(mouseDragOrigin);
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {
            data.regionRootCoordinate = default;
            data.previewer = null;
            data.anchorPreviewers = null;
        }

        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
        }
    }
}
