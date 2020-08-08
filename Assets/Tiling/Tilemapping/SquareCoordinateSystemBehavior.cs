using Assets.Tiling.SquareCoords;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public class SquareCoordinateSystemBehavior : MonoBehaviour
    {
        public CoordinateSystemTransform<SquareCoordinate> coordinateSystem;

        public void Awake()
        {
            var basis = new SquareCoordinateSystem();
            coordinateSystem = new CoordinateSystemTransform<SquareCoordinate>(basis, transform);
        }
    }
}
