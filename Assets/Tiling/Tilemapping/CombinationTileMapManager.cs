using System.Linq;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public struct SubCoordinateSystem
    {
        public CoordinateSystemBehavior<ICoordinate> coordinateSystem;
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

    }
}
