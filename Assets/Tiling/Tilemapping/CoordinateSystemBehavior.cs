using System.Collections.Generic;
using UnityEngine;

namespace Assets.Tiling.Tilemapping
{
    public abstract class CoordinateSystemBehavior<T> : MonoBehaviour where T : ICoordinate
    {
        public ICoordinateSystem<T> coordinateSystem;
        public float sideLength;
        public float tilePadding;

        public void Awake()
        {
            var basis = BaseCoordinateSystem();
            coordinateSystem = new CoordinateSystemTransform<T>(basis, transform);
        }

        protected abstract ICoordinateSystem<T> BaseCoordinateSystem();
    }
}
