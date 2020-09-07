using Assets.Scripts.Core;
using Assets.Tiling.Tilemapping;
using Assets.WorldObjects;
using UniRx;
using UnityEngine;

namespace Assets.UI.Scripts
{
    [RequireComponent(typeof(CombinationTileMapManager))]
    public class TilemapMouseTracker : MonoBehaviour
    {
        public GameObjectVariable objectToTrack;

        private TileMapMember trackingMemeber;
        private CombinationTileMapManager combinationManager;

        public void Awake()
        {
            combinationManager = GetComponent<CombinationTileMapManager>();

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
            var tilemapLocation = combinationManager.GetPositionInTileMaps(mousePos);
            if (!tilemapLocation.HasValue)
            {
                return;
            }
            trackingMemeber.SetPosition(tilemapLocation.Value.coordinateInMap, tilemapLocation.Value.tileMap);
        }
    }
}