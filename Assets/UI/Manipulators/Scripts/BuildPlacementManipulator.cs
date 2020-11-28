using Assets.Tiling;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects;
using System.Linq;
using UnityEngine;

namespace Assets.UI.Manipulators
{
    [CreateAssetMenu(fileName = "BuildPlacementManipulator", menuName = "TileMap/Manipulators/BuildPlacementManipulator", order = 1)]
    public class BuildPlacementManipulator : MapManipulator
    {
        private ManipulatorController controller;

        public TileMapMember buildPreviewPrefab;
        public TileMapMember buildOrderMemeberPrefab;
        public LayerMask blockingLayers;

        private TileMapMember activeBuildPreview;

        public override void OnOpen(ManipulatorController controller)
        {
            Debug.Log("opening build manipulator");
            this.controller = controller;
            activeBuildPreview = GameObject.Instantiate(buildPreviewPrefab, controller.transform);
            currentHoverCoordinate = default;
        }

        public override void OnClose()
        {
            Destroy(activeBuildPreview.gameObject);
        }

        private UniversalCoordinate currentHoverCoordinate;
        private bool blocked;

        public override void OnUpdate()
        {
            UpdatePreviewPositionAndBlocking();
            if (currentHoverCoordinate.IsValid() && !blocked && Input.GetMouseButtonDown(0))
            {
                var newBuildableObject = GameObject.Instantiate(buildOrderMemeberPrefab, CombinationTileMapManager.instance.transform);
                newBuildableObject.SetPosition(currentHoverCoordinate);

                blocked = true;
                activeBuildPreview.gameObject.SetActive(false);
            }
        }

        private void UpdatePreviewPositionAndBlocking()
        {
            var posInWorld = MyUtilities.GetMousePos2D();
            var hoveredCoordinate = CombinationTileMapManager.instance.GetValidCoordinateFromWorldPosIfExists(posInWorld);
            if (!hoveredCoordinate.HasValue || !hoveredCoordinate.Value.IsValid())
            {
                currentHoverCoordinate = default;
                activeBuildPreview.gameObject.SetActive(false);
                return;
            }
            if (hoveredCoordinate.Equals(currentHoverCoordinate))
            {
                return;
            }
            currentHoverCoordinate = hoveredCoordinate.Value;

            var itemsAtHover = CombinationTileMapManager.instance.everyMember.GetMembersOnTile(currentHoverCoordinate);
            blocked = itemsAtHover
                .Where(obj => (blockingLayers & (1 << obj.gameObject.layer)) != 0)
                .Any();
            if (blocked)
            {
                activeBuildPreview.gameObject.SetActive(false);
            }
            else
            {
                activeBuildPreview.gameObject.SetActive(true);
                activeBuildPreview.SetPosition(currentHoverCoordinate);
            }
        }
    }
}
