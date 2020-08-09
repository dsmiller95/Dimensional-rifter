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
            BakeAllTileMapMeshes();
        }

        private void BakeAllTileMapMeshes(params TileMapRegionNoCoordinateType[] exclude)
        {
            var allTileMaps = GetComponentsInChildren<TileMapRegionNoCoordinateType>()
                .Where(x => !exclude.Contains(x))
                .ToArray();
            if (allTileMaps == null || allTileMaps.Length <= 0)
            {
                return;
            }
            var allColliders = allTileMaps
                .Select(x => x.SetupBoundingCollider())
                .ToList();

            for (var i = 0; i < allTileMaps.Length; i++)
            {
                var tileMap = allTileMaps[i];
                tileMap.BakeMeshAvoidingColliders(allColliders.Skip(i + 1));
            }
        }

        private Vector2 lastMousePos;
        public TileMapRegionNoCoordinateType tileMapToMove;

        public TileMapRegionNoCoordinateType tileMapPrefab;
        private void Update()
        {
            var currentMousePos = Utilities.GetMousePos2D();
            var mouseDelta = currentMousePos - lastMousePos;
            lastMousePos = currentMousePos;


            if (Input.GetKeyDown(KeyCode.A) && !isPlacingTileMap)
            {
                var newTileMap = Instantiate(tileMapPrefab, transform).GetComponent<TileMapRegionNoCoordinateType>();
                newTileMap.BakeMeshAvoidingColliders(null);
                tileMapToMove = newTileMap;
                BeginMovingTileMap(tileMapToMove);
            }
            else if (Input.GetMouseButtonDown(0) && isPlacingTileMap)
            {
                FinishMovingTileMap();
            }

            if (isPlacingTileMap)
            {
                tileMapToMove.transform.position += (Vector3)(mouseDelta);
                UpdateTileMapsBelow(tileMapToMove);
                // do some placement
            }
        }

        private bool isPlacingTileMap;
        //private TileMapRegionNoCoordinateType tileMapToMove;

        public void BeginMovingTileMap(TileMapRegionNoCoordinateType tileMap)
        {
            isPlacingTileMap = true;
            BakeAllTileMapMeshes(tileMap);

            // tileMapToMove = Instantiate(tileMapPrefab, transform);//todo: make a new tile map?
        }

        public void FinishMovingTileMap()
        {
            isPlacingTileMap = false;
            BakeAllTileMapMeshes();

            //TODO: so some thing to finalize?
            //tileMapToMove = null;
        }

        private void UpdateTileMapsBelow(TileMapRegionNoCoordinateType tileMap)
        {
            var index = tileMap.transform.GetSiblingIndex();
            var belowTileMaps = transform.GetComponentsInChildren<TileMapRegionNoCoordinateType>().Take(index).ToArray();

            var currentTileMapCollider = tileMap.SetupBoundingCollider();
            var colliders = new[] { currentTileMapCollider };

            foreach (var tileMapToUpdate in belowTileMaps)
            {
                tileMapToUpdate.UpdateMeshTilesBasedOnColliders(colliders);
            }
        }

    }
}
