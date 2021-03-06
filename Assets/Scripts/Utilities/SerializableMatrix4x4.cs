﻿using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    [Serializable]
    public class SerializableMatrix4x4
    {
        private float[] matrixData;
        public SerializableMatrix4x4(Matrix4x4 matrix)
        {
            matrixData = new[] { 0, 1, 2, 3 }
                .Select(index => matrix.GetColumn(index))
                .SelectMany(ToFloatStream)
                .ToArray();
        }

        private static IEnumerable<float> ToFloatStream(Vector4 vector)
        {
            yield return vector.x;
            yield return vector.y;
            yield return vector.z;
            yield return vector.w;
        }

        private static IEnumerable<Vector4> FromFloatStream(IEnumerable<float> self)
        {
            return self.Buffer(4).Select(floats => new Vector4(
                floats[0],
                floats[1],
                floats[2],
                floats[3]
                ));
        }

        public Matrix4x4 GetMatrix()
        {
            var newMatrix = new Matrix4x4();
            foreach (var vect in FromFloatStream(matrixData).Select((vect, i) => new { vect, i }))
            {
                newMatrix.SetColumn(vect.i, vect.vect);
            }
            return newMatrix;
        }
    }
}
