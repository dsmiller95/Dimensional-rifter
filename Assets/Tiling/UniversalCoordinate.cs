﻿using Assets.Tiling.SquareCoords;
using Assets.Tiling.TriangleCoords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Unity.Collections;
using UnityEngine;

namespace Assets.Tiling
{
    public enum CoordinateType : short
    {
        INVALID = 0,
        TRIANGLE,
        SQUARE
    }

    public interface IBaseCoordinateType
    {

    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct UniversalCoordinate : ISerializable, IEquatable<UniversalCoordinate>
    {
        public static int MaxNeighborCount = 6;

        // all data views should take up no more than 3 ints, no more that 12 bytes
        [FieldOffset(0)] private int coordinateDataPartOne;
        [FieldOffset(4)] private int coordinateDataPartTwo;
        [FieldOffset(8)] private int coordinateDataPartThree;

        /// <summary>
        /// view of coordinate data as a triangle coordinate. Only use after checking that <see cref="type"/> is of type Triangle
        /// </summary>
        [FieldOffset(0)] public TriangleCoordinateStructSystem triangleDataView;
        [FieldOffset(0)] public SquareCoordinate squareDataView;


        /// <summary>
        /// composite of both the type of the coordinate and the plane it belongs to. Can be used to compare if two coordinates
        ///     are equal in both type and the plane they are rendered on
        /// </summary>
        [FieldOffset(12)] public int CoordinateMembershipData;
        [FieldOffset(12)] public CoordinateType type;
        [FieldOffset(14)] public short CoordinatePlaneID;

        public static UniversalCoordinate From(TriangleCoordinateStructSystem b, short CoordinatePlaneID)
        {
            return new UniversalCoordinate
            {
                triangleDataView = b,
                type = CoordinateType.TRIANGLE,
                CoordinatePlaneID = CoordinatePlaneID
            };
        }
        public static UniversalCoordinate From(SquareCoordinate b, short CoordinatePlaneID)
        {
            return new UniversalCoordinate
            {
                squareDataView = b,
                type = CoordinateType.SQUARE,
                CoordinatePlaneID = CoordinatePlaneID
            };
        }

        public Vector2 ToPositionInPlane()
        {
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    return triangleDataView.ToPositionInPlane();
                case CoordinateType.SQUARE:
                    return squareDataView.ToPositionInPlane();
                default:
                    return default;
            }
        }
        public static UniversalCoordinate FromPositionInPlane(Vector2 pos, CoordinateType type, short coordinatePlaneID)
        {
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    return From(TriangleCoordinateStructSystem.FromPositionInPlane(pos), coordinatePlaneID);
                case CoordinateType.SQUARE:
                    return From(SquareCoordinate.FromPositionInPlane(pos), coordinatePlaneID);
                default:
                    return default;
            }
        }
        public IEnumerable<Vector2> GetVertexesAround()
        {
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    return triangleDataView.GetTriangleVertextesAround();
                case CoordinateType.SQUARE:
                    return squareDataView.GetSquareVertsAround();
                default:
                    return null;
            }
        }
        public Bounds GetRawBounds(float sideLength, Matrix4x4 systemTransform)
        {
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    return triangleDataView.GetRawBounds(sideLength, systemTransform);
                case CoordinateType.SQUARE:
                    return squareDataView.GetRawBounds(sideLength, systemTransform);
                default:
                    return default;
            }
        }

        public static UniversalCoordinate GetDefault(CoordinateType coordType)
        {
            switch (coordType)
            {
                case CoordinateType.TRIANGLE:
                    return From(TriangleCoordinateStructSystem.AtOrigin(), 0);
                case CoordinateType.SQUARE:
                    return From(SquareCoordinate.AtOrigin(), 0);
                default:
                    return default;
            }
        }

        public static int[] GetTileTriangleIDs(CoordinateType coordType)
        {
            switch (coordType)
            {
                case CoordinateType.TRIANGLE:
                    return TriangleCoordinateStructSystem.GetTileTriangleIDs();
                case CoordinateType.SQUARE:
                    return SquareCoordinate.GetTileTriangleIDs();
                default:
                    return default;
            };
        }

        public bool IsValid()
        {
            return type != CoordinateType.INVALID;
        }

        public float HeuristicDistance(UniversalCoordinate other)
        {
            if (other.CoordinateMembershipData != CoordinateMembershipData)
            {
                switch (type)
                {
                    case CoordinateType.TRIANGLE:
                        return TriangleCoordinateStructSystem.HeuristicDistance(triangleDataView, other.triangleDataView);
                    case CoordinateType.SQUARE:
                        return SquareCoordinate.HeuristicDistance(squareDataView, other.squareDataView);
                    default:
                        break;
                }
            }
            return -1;
        }

        public IEnumerable<UniversalCoordinate> Neighbors()
        {
            var myPlaneID = CoordinatePlaneID;
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    return triangleDataView.Neighbors().Select(x => From(x, myPlaneID));
                case CoordinateType.SQUARE:
                    return squareDataView.Neighbors().Select(x => From(x, myPlaneID));
                default:
                    return null;
            }
        }

        public void SetNeighborsIntoSwapSpace(NativeArray<UniversalCoordinate> swapSpace)
        {
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    triangleDataView.SetNeighborsIntoSwapSpace(swapSpace, CoordinatePlaneID);
                    return;
                case CoordinateType.SQUARE:
                    squareDataView.SetNeighborsIntoSwapSpace(swapSpace, CoordinatePlaneID);
                    return;
                default:
                    break;
            }
        }
        public int NeighborCount()
        {
            return NeighborCount(type);
        }
        public static int NeighborCount(CoordinateType type)
        {
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    return 3;
                case CoordinateType.SQUARE:
                    return 4;
                default:
                    return -1;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("data1", coordinateDataPartOne);
            info.AddValue("data2", coordinateDataPartTwo);
            info.AddValue("data3", coordinateDataPartThree);
            info.AddValue("membership", CoordinateMembershipData);
        }
        // The special constructor is used to deserialize values.
        private UniversalCoordinate(SerializationInfo info, StreamingContext context)
        {
            triangleDataView = default;
            squareDataView = default;
            type = CoordinateType.INVALID;
            CoordinatePlaneID = default;

            coordinateDataPartOne = info.GetInt32("data1");
            coordinateDataPartTwo = info.GetInt32("data2");
            coordinateDataPartThree = info.GetInt32("data3");
            CoordinateMembershipData = info.GetInt32("membership");
        }

        public override string ToString()
        {
            switch (type)
            {
                case CoordinateType.TRIANGLE:
                    return $"Triangle: {triangleDataView}, in plane {CoordinatePlaneID}";
                case CoordinateType.SQUARE:
                    return $"Square: {squareDataView}, in plane {CoordinatePlaneID}";
                default:
                    return "Invalid coordinate";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is UniversalCoordinate typedOther)
            {
                return Equals(typedOther);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + coordinateDataPartOne;
            hash = hash * 23 + coordinateDataPartTwo;
            hash = hash * 23 + coordinateDataPartThree;
            hash = hash * 23 + CoordinateMembershipData;
            return hash;
        }

        public bool Equals(UniversalCoordinate other)
        {
            return other.CoordinateMembershipData == CoordinateMembershipData &&
                other.coordinateDataPartOne == coordinateDataPartOne &&
                other.coordinateDataPartTwo == coordinateDataPartTwo &&
                other.coordinateDataPartThree == coordinateDataPartThree;
        }
    }
}
