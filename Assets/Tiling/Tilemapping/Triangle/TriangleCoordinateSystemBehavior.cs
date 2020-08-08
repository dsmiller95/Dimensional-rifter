using Assets.Tiling.TriangleCoords;
using UnityEngine;

namespace Assets.Tiling.Tilemapping.Triangle
{
    public class TriangleCoordinateSystemBehavior : MonoBehaviour
    {
        public CoordinateSystemTransform<TriangleCoordinate> coordinateSystem;

        public void Awake()
        {
            var basis = new TriangleCoordinateSystem();
            coordinateSystem = new CoordinateSystemTransform<TriangleCoordinate>(basis, transform);
        }
    }
}
