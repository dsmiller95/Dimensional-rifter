using Assets.Tiling.SquareCoords;
using Assets.Tiling.TriangleCoords;
using Assets.WorldObjects;
using Simulation.Tiling.HexCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Tiling
{
    public class UniversalToGenericAdaptors
    {

        public static IEnumerable<ICoordinate> PathBetween(ICoordinate source, ICoordinate destination, ICoordinateSystem coordinateSystem)
        {
            if (!coordinateSystem.IsCompatible(source) || !coordinateSystem.IsCompatible(destination))
            {
                throw new ArgumentException("coordinates not compatable");
            }

            switch (coordinateSystem.CoordType)
            {
                case CoordinateSystemType.HEX:
                    return PathBetween<AxialCoordinate>(source, destination, coordinateSystem)
                        .Cast<ICoordinate>();
                case CoordinateSystemType.SQUARE:
                    return PathBetween<SquareCoordinate>(source, destination, coordinateSystem)
                        .Cast<ICoordinate>();
                case CoordinateSystemType.TRIANGLE:
                    return PathBetween<TriangleCoordinate>(source, destination, coordinateSystem)
                        .Cast<ICoordinate>();
            }
            throw new ArgumentException("invalid coordinate system type");
        }

        private static IEnumerable<T> PathBetween<T>(ICoordinate source, ICoordinate destination, ICoordinateSystem coordinateSystem) where T : ICoordinate
        {
            var pather = new Pathfinder<T>((T)destination, coordinateSystem as ICoordinateSystem<T>);
            return pather.ShortestPathTo((T)source);
        }

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
    }
}
