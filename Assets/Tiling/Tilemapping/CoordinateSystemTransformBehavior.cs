using UnityEngine;

namespace Assets.Tiling.Tilemapping
{

    public abstract class CoordinateSystemTransformBehavior<T> : MonoBehaviour where T : ICoordinate
    {
        public ICoordinateSystem<T> coordinateSystem;

        public void Awake()
        {
            var basis = BaseCoordinateSystem;
            coordinateSystem = new CoordinateSystemTransform<T>(basis, transform);
        }

        public abstract ICoordinateSystem<T> BaseCoordinateSystem { get; }
    }
}
