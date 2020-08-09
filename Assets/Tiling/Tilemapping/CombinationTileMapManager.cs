using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public struct SubCoordinateSystem
    {
        public CoordinateSystemTransformBehavior<ICoordinate> coordinateSystem;
    }

    public class CombinationTileMapManager : MonoBehaviour
    {
        private void Start()
        {
            var allTileMaps = GetComponentsInChildren<TileMapRegionNoCoordinateType>();
            Debug.Log(allTileMaps.Length);
            if(allTileMaps == null || allTileMaps.Length <= 0)
            {
                return;
            }
            var allColliders = allTileMaps
                .Select(x => x.SetupBoundingCollider())
                .ToList();

            for (var i = 0; i < allTileMaps.Length; i++)
            {
                var tileMap = allTileMaps[i];
                tileMap.SetupMyMesh(allColliders.Skip(i + 1));
            }
        }


        private bool isPlacingTileMap;
        public TileMapRegionNoCoordinateType tileMapPrefab;
        private TileMapRegionNoCoordinateType currentPlacingTileMap;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A) && !isPlacingTileMap)
            {
                this.BeginTileMapPlace();
            }else if (Input.GetMouseButtonDown(0) && isPlacingTileMap)
            {
                this.FinishTileMapPlace();
            }else if (isPlacingTileMap)
            {
                // do some placement
            }
        }

        public void BeginTileMapPlace()
        {
            isPlacingTileMap = true;
            currentPlacingTileMap = Instantiate(tileMapPrefab, transform);//todo: make a new tile map?
        }

        public void FinishTileMapPlace()
        {
            isPlacingTileMap = false;

            //TODO: so some thing to finalize?

            currentPlacingTileMap = null;
        }

    }
}
