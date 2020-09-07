using Assets.Tiling.SquareCoords;
using Assets.Tiling.TriangleCoords;
using Simulation.Tiling.HexCoords;
using System;
using UnityEngine;

namespace Assets.Tiling
{
    public class UniversalToGenericAdaptors
    {

        public static Vector2 ToRealPosition(ICoordinate source, ICoordinateSystem coordinateSystem)
        {
            if (!coordinateSystem.IsCompatible(source))
            {
                throw new ArgumentException("coordinates not compatable");
            }

            switch (coordinateSystem.CoordType)
            {
                case CoordinateSystemType.HEX:
                    return RealPosition<AxialCoordinate>(source, coordinateSystem);
                case CoordinateSystemType.SQUARE:
                    return RealPosition<SquareCoordinate>(source, coordinateSystem);
                case CoordinateSystemType.TRIANGLE:
                    return RealPosition<TriangleCoordinate>(source, coordinateSystem);
            }
            throw new ArgumentException("invalid coordinate system type");
        }

        private static Vector2 RealPosition<T>(ICoordinate source, ICoordinateSystem coordinateSystem) where T : ICoordinate
        {
            return (coordinateSystem as ICoordinateSystem<T>).ToRealPosition((T)source);
        }

        public static ICoordinateSystem<T> GetBasicCoordinateSystemFromType<T>(CoordinateSystemType type) where T : ICoordinate
        {
            ICoordinateSystem coordinateSystemResult = null;
            switch (type)
            {
                case CoordinateSystemType.HEX:
                    coordinateSystemResult = new HexCoordinateSystem(1);
                    break;
                case CoordinateSystemType.SQUARE:
                    coordinateSystemResult = new SquareCoordinateSystem();
                    break;
                case CoordinateSystemType.TRIANGLE:
                    coordinateSystemResult = new TriangleCoordinateSystem();
                    break;
            }
            if (coordinateSystemResult != null && coordinateSystemResult is ICoordinateSystem<T> castedCoords)
            {
                return castedCoords;
            }
            throw new Exception("Coordinate system type does not match generic parameter");
        }
    }
}
