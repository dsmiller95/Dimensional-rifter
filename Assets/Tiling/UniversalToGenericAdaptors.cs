using Assets.Tiling.SquareCoords;
using Assets.Tiling.Tilemapping;
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

        public static IEnumerable<ICoordinate> PathBetween(
            ICoordinate source,
            ICoordinate destination,
            TileMapRegionNoCoordinateType region,
            Func<TileProperties, bool> passableTiles)
        {
            var coordinateSpace = region.UntypedCoordianteSystemWorldSpace;

            if (!coordinateSpace.IsCompatible(source) || !coordinateSpace.IsCompatible(destination))
            {
                throw new ArgumentException("coordinates not compatable");
            }

            switch (coordinateSpace.CoordType)
            {
                case CoordinateSystemType.HEX:
                    return PathBetween<AxialCoordinate>(source, destination, region, passableTiles)?
                        .Cast<ICoordinate>();
                case CoordinateSystemType.SQUARE:
                    return PathBetween<SquareCoordinate>(source, destination, region, passableTiles)?
                        .Cast<ICoordinate>();
                case CoordinateSystemType.TRIANGLE:
                    return PathBetween<TriangleCoordinate>(source, destination, region, passableTiles)?
                        .Cast<ICoordinate>();
            }
            throw new ArgumentException("invalid coordinate system type");
        }

        private static IEnumerable<T> PathBetween<T>(ICoordinate source, ICoordinate destination, TileMapRegionNoCoordinateType coordinateSystem, Func<TileProperties, bool> passableTiles) where T : ICoordinate
        {
            var coordSystem = (coordinateSystem as TileMapRegion<T>);

            var pather = new Pathfinder<T>((T)destination, coordSystem, (coord, properties) => passableTiles(properties));
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

        public static ICoordinateSystem<T> GetBasicCoordinateSystemFromType<T>(CoordinateSystemType type) where T: ICoordinate
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
            if(coordinateSystemResult != null && coordinateSystemResult is ICoordinateSystem<T> castedCoords)
            {
                return castedCoords;
            }
            throw new Exception("Coordinate system type does not match generic parameter");
        }
    }
}
