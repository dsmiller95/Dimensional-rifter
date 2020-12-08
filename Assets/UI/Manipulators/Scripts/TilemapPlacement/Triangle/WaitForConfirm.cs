using Assets.Behaviors.Scripts;
using Assets.Tiling.Tilemapping;
using UnityEngine;

namespace Assets.UI.Manipulators.Scripts.TilemapPlacement.Triangle
{
    public class WaitForConfirm : IGenericStateHandler<TriangleTileMapPlacementManipulator>
    {
        private bool Confirmed;
        private bool Canceled;

        public IGenericStateHandler<TriangleTileMapPlacementManipulator> HandleState(TriangleTileMapPlacementManipulator data)
        {
            if (Confirmed)
            {
                Debug.Log("confirm");
                var previewBehavior = CombinationTileMapManager.instance.GetPreviewBehavior(data.regionRootCoordinate.CoordinatePlaneID);
                GameObject.Destroy(previewBehavior.confirmUIParent);
                return new DragStartDetectState();
            }
            if (Canceled)
            {
                Debug.Log("cancel");
                CombinationTileMapManager.instance.ClosePreviewRegion(data.regionRootCoordinate.CoordinatePlaneID);
                return new DragStartDetectState();
            }
            return this;
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {
            var previewBehavior = CombinationTileMapManager.instance.GetPreviewBehavior(data.regionRootCoordinate.CoordinatePlaneID);
            previewBehavior.ConfirmButton.onClick.AddListener(OnConfirm);
            previewBehavior.CancelButton.onClick.AddListener(OnCancel);
        }
        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
            var previewBehavior = CombinationTileMapManager.instance.GetPreviewBehavior(data.regionRootCoordinate.CoordinatePlaneID);
            previewBehavior?.ConfirmButton.onClick.RemoveListener(OnConfirm);
            previewBehavior?.CancelButton.onClick.RemoveListener(OnCancel);
        }

        private void OnConfirm()
        {
            Confirmed = true;
        }
        private void OnCancel()
        {
            Canceled = true;
        }

    }
}
