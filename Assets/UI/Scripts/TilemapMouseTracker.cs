using Assets.Scripts.Core;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects;
using UniRx;
using UnityEngine;

namespace Assets.UI.Scripts
{
    [RequireComponent(typeof(TheReelBigCombinationTileMapManager))]
    public class TilemapMouseTracker : MonoBehaviour
    {
        public GameObjectVariable objectToTrack;

        private TileMapMember trackingMemeber;
        private TheReelBigCombinationTileMapManager combinationManager;

        public void Awake()
        {
            combinationManager = GetComponent<TheReelBigCombinationTileMapManager>();

            SetTracking(objectToTrack.CurrentValue);
            objectToTrack.Value.TakeUntilDestroy(this)
                .Subscribe(newGameObject => SetTracking(newGameObject));
        }

        private void SetTracking(GameObject o)
        {
            if (!o || o == null)
            {
                trackingMemeber = null;
            }
            else
            {
                trackingMemeber = o.GetComponent<TileMapMember>();
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (!trackingMemeber || trackingMemeber == null)
            {
                return;
            }
            var mousePos = MyUtilities.GetMousePos2D();
            var tilemapLocation = combinationManager.GetPositionOnActiveTileMapsFromWorldPosition(mousePos);
            if (!tilemapLocation.HasValue)
            {
                return;
            }
            trackingMemeber.SetPosition(tilemapLocation.Value);
        }
    }
}