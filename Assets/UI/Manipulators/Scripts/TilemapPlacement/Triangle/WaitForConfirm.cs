using Assets.Behaviors.Scripts;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects.Members.Buildings.DOTS.Anchor;
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
                GameObject.Destroy(data.previewer.confirmUIParent);
                foreach (var anchor in data.anchorPreviewers)
                {
                    var newAnchor = GameObject.Instantiate(data.anchorBuildPrefab, CombinationTileMapManager.instance.transform);
                    newAnchor.SetPosition(anchor);
                    var anchoringIndex = newAnchor.GetComponent<TilemapAnchorComponentAuthoring>();
                    anchoringIndex.AnchoredTileMapIndex = -1;
                    GameObject.Destroy(anchor.gameObject);
                }
                return new DragStartDetectState();
            }
            if (Canceled)
            {
                Debug.Log("cancel");
                CombinationTileMapManager.instance.ClosePreviewRegion(data.previewer);
                foreach (var anchor in data.anchorPreviewers)
                {
                    GameObject.Destroy(anchor.gameObject);
                }
                return new DragStartDetectState();
            }
            return this;
        }

        public void TransitionIntoState(TriangleTileMapPlacementManipulator data)
        {
            var previewBehavior = data.previewer;
            previewBehavior.ConfirmButton.onClick.AddListener(OnConfirm);
            previewBehavior.CancelButton.onClick.AddListener(OnCancel);
        }
        public void TransitionOutOfState(TriangleTileMapPlacementManipulator data)
        {
            var previewBehavior = data.previewer;
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
