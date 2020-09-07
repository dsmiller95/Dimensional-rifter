using UnityEngine;

namespace Assets.Tiling.Tilemapping
{

    public abstract class CoordinateSystemTransformBehavior<T> : MonoBehaviour where T : ICoordinate
    {
        private ICoordinateSystem<T> _transCoordSystem;

        public ICoordinateSystem<T> TransformedCoordinateSystem
        {
            get
            {
                if (_transCoordSystem == null)
                {
                    _transCoordSystem = new CoordinateSystemTransform<T>(BaseCoordinateSystem, transform);
                }
                return _transCoordSystem;
            }
        }
        public abstract ICoordinateSystem<T> BaseCoordinateSystem { get; }
    }
}
