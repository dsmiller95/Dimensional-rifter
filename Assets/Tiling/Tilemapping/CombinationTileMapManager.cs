using Assets.WorldObjects.SaveObjects;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    [Serializable]
    public struct TileMapTypePrefabConfig
    {
        public CoordinateSystemType type;
        public TileMapRegionNoCoordinateType tileMapPrefab;
    }

    [Serializable]
    public struct CompleteTileMapPosition
    {
        public ICoordinate coordinateInMap;
        public TileMapRegionNoCoordinateType tileMap;
    }

    public class CombinationTileMapManager : MonoBehaviour, ISaveable<WorldSaveObject>
    {
        public TileMapTypePrefabConfig[] tileMapTypes;

        private void Start()
        {
            BakeAllTileMapMeshes();
        }

        private void BakeAllTileMapMeshes(params TileMapRegionNoCoordinateType[] exclude)
        {
            var allTileMaps = GetComponentsInChildren<TileMapRegionNoCoordinateType>()
                .Where(x => x.gameObject.activeInHierarchy)
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
                tileMap.BakeTopologyAvoidingColliders(allColliders.Skip(i + 1));
            }
        }

        private Vector2 lastMousePos;
        public TileMapRegionNoCoordinateType tileMapToMove;

        public TileMapRegionNoCoordinateType tileMapPrefab;
        private void Update()
        {
            var currentMousePos = MyUtilities.GetMousePos2D();
            var mouseDelta = currentMousePos - lastMousePos;
            lastMousePos = currentMousePos;

            if (Input.GetKeyDown(KeyCode.A) && !isPlacingTileMap)
            {
                var newTileMap = Instantiate(tileMapPrefab, transform).GetComponent<TileMapRegionNoCoordinateType>();
                newTileMap.BakeTopologyAvoidingColliders(null);
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
            }
        }

        public CompleteTileMapPosition? GetPositionInTileMaps(Vector2 worldPosition)
        {
            return GetComponentsInChildren<TileMapRegionNoCoordinateType>()
                .Where(x => x.gameObject.activeInHierarchy)
                .Select(x => x.GetCoordinatesFromWorldSpaceIfValid(worldPosition))
                .Where(x => x.HasValue)
                .FirstOrDefault();
        }


        private bool isPlacingTileMap;

        public void BeginMovingTileMap(TileMapRegionNoCoordinateType tileMap)
        {
            isPlacingTileMap = true;
            BakeAllTileMapMeshes(tileMap);
        }

        public void FinishMovingTileMap()
        {
            isPlacingTileMap = false;
            BakeAllTileMapMeshes();
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

        public WorldSaveObject GetSaveObject()
        {
            var allRegions = GetComponentsInChildren<TileMapRegionNoCoordinateType>()
                .Where(x => x.gameObject.activeInHierarchy);

            return new WorldSaveObject
            {
                regions = allRegions.Select(x => x.GetSaveObject()).ToList()
            };
        }

        public void SetupFromSaveObject(WorldSaveObject save)
        {
            foreach (var region in save.regions)
            {
                var prefab = tileMapTypes.Where(x => x.type == region.tileType).First().tileMapPrefab;
                var newTileMap = Instantiate(prefab, transform).GetComponent<TileMapRegionNoCoordinateType>();
                newTileMap.SetupFromSaveObject(region);
            }
        }

    }
}
