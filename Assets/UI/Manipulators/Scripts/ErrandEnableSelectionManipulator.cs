using Assets.Behaviors.Errands.Scripts;
using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
using UnityEngine;

namespace Assets.UI.Manipulators
{

    [CreateAssetMenu(fileName = "ErrandEnableManipulator", menuName = "TileMap/Manipulators/EnableErrandManipulator", order = 1)]
    public class ErrandEnableSelectionManipulator : MapManipulator
    {
        private ManipulatorController controller;

        public ErrandType typeOfErrand;

        public override void OnOpen(ManipulatorController controller)
        {
            Debug.Log("opening manipulator");
            this.controller = controller;
            firstCoordinate = default;
        }

        public override void OnClose()
        {
            range = default;
        }

        private UniversalCoordinate firstCoordinate;

        private RectCoordinateRange range;

        public override void OnUpdate()
        {
            if (firstCoordinate.IsValid())
            {
                if (Input.GetMouseButton(0))
                {
                    // dragging
                    var posInWorld = MyUtilities.GetMousePos2D();
                    var hoveredOverCoord = CombinationTileMapManager.instance.GetCoordinateOnSamePlane(posInWorld, firstCoordinate);
                    var newRange = RectCoordinateRange.FromCoordsInclusive(firstCoordinate.squareDataView, hoveredOverCoord.squareDataView);

                    if (range == null || range != newRange)
                    {
                        Debug.Log("Range changed");
                        range = newRange;
                        Debug.Log(range);
                    }
                }
                else
                {
                    // dragging done
                    Debug.Log(range);
                    range = default;
                    firstCoordinate = default;
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                // drag start
                var posInWorld = MyUtilities.GetMousePos2D();
                var hitCoordinate = CombinationTileMapManager.instance.GetValidCoordinateFromWorldPosIfExists(posInWorld);
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
