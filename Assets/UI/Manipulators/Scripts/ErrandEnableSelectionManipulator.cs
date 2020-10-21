﻿using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using Assets.UI.Manipulators.Scripts;
using UnityEngine;

namespace Assets.UI.Manipulators
{

    [CreateAssetMenu(fileName = "ErrandEnableManipulator", menuName = "TileMap/Manipulators/EnableErrandManipulator", order = 1)]
    public class ErrandEnableSelectionManipulator : MapManipulator
    {
        private ManipulatorController controller;
        private SelectedAreaVisualizer areaVisualizer;

        public override void OnOpen(ManipulatorController controller)
        {
            Debug.Log("opening manipulator");
            this.controller = controller;
            firstCoordinate = default;
            areaVisualizer = controller.GetComponent<SelectedAreaVisualizer>();
        }

        public override void OnClose()
        {
            range = default;
        }

        private UniversalCoordinate firstCoordinate;

        private SquareCoordinateRange range;

        public override void OnUpdate()
        {
            if (firstCoordinate.IsValid())
            {
                if (Input.GetMouseButton(0))
                {
                    // dragging
                    var posInWorld = MyUtilities.GetMousePos2D();
                    var hoveredOverCoord = CombinationTileMapManager.instance.GetCoordinateOnPlaneIDNoValidCheck(posInWorld, firstCoordinate);
                    var newRange = SquareCoordinateRange.FromCoordsInclusive(firstCoordinate.squareDataView, hoveredOverCoord.squareDataView);

                    if (range == null || !range.Equals(newRange))
                    {
                        Debug.Log("Range changed");
                        range = newRange;
                        Debug.Log(range);
                        areaVisualizer.RenderRange(range, firstCoordinate.CoordinatePlaneID);
                    }
                }
                else
                {
                    // dragging done
                    Debug.Log(range);
                    range = default;
                    firstCoordinate = default;
                    areaVisualizer.StopRenderRange();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                // drag start
                var posInWorld = MyUtilities.GetMousePos2D();
                var hitCoordinate = CombinationTileMapManager.instance.GetPositionOnActiveTileMapsFromWorldPosition(posInWorld);
                Debug.Log(hitCoordinate);
                if (!hitCoordinate.HasValue || hitCoordinate.Value.type != CoordinateType.SQUARE)
                {
                    return;
                }
                firstCoordinate = hitCoordinate.Value;
            }
        }
    }
}
