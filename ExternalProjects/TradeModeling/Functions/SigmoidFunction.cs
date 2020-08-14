using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TradeModeling.Functions
{
    [Serializable]
    public struct SigmoidFunctionConfig
    {
        /// <summary>
        /// The range across which the sigmoid function should exhibit most of its variance
        ///     a range of [0, 1] maps to a range of [-5, 5] on the sigmoid function, when offset is 0
        /// </summary>
        public float range;
        /// <summary>
        /// The offset from the center of the range where the sigmoid function should cross 0.5
        /// </summary>
        public float offset;
        /// <summary>
        /// The range of values that should be represented in the output of the function. at a value of one
        ///     the function's domain is (0, 1), at 3 it is (0, 3), etc
        /// </summary>

        public float yRange;
    }

    public class SigmoidFunction : ISingleFunction, IIncrementalFunction
    {
        //public float range = 1;
        //public float offset = 0;

        /// <summary>
        /// A feature of the sigmoid function is that the limit of its integral function
        ///     is equal to the offset from 0
        /// </summary>
        private float integralLimit => realOffset + integralOffset;
        private float realOffset;
        private float xScale;
        private float yScale;

        private float integralOffset;

        public SigmoidFunction(SigmoidFunctionConfig config)
        {
            var range = config.range;
            var offset = config.offset;

            realOffset = offset + (range / 2);
            xScale = 10 / range;
            yScale = config.yRange;

            integralOffset = (float)Math.Log(Math.Pow(Math.E, -xScale * realOffset) + 1f) / xScale;
        }

        public float GetValueAtPoint(float point)
        {
            return Sigmoid(point, realOffset, xScale, yScale);

        }
        public float GetIncrementalValue(float startPoint, float increment)
        {
            return GetNetValue(startPoint + increment) - GetNetValue(startPoint);
        }

        public float GetNetValue(float startPoint)
        {
            return SigmoidIntegral(startPoint, realOffset, xScale, yScale, integralOffset);
        }

        public float GetPointFromNetValue(float value)
        {
            return InverseSigmoidIntegral(value, realOffset, xScale, yScale, integralOffset);
        }

        public float GetPointFromNetExtraValueFromPoint(float extraValue, float startPoint)
        {
            var startValue = GetNetValue(startPoint);
            if (startValue + extraValue >= integralLimit) {
                return float.MaxValue;
            }
            return this.GetPointFromNetValue(startValue + extraValue);
        }

        public static float Sigmoid(double value, float offset, float xScale, float yScale)
        {
            return (float)(1.0 / (1.0 + Math.Pow(Math.E, -(offset - value) * xScale))) * yScale;
        }
        public static float SigmoidIntegral(double value, float offset, float xScale, float yScale, float integralOffset)
        {
            return (float)(
                value
                - (Math.Log(
                    1 +
                    Math.Pow(
                        Math.E,
                        (value - offset) * xScale)
                    ) / xScale)
                + integralOffset
                ) * yScale;
        }
        public static float InverseSigmoidIntegral(double value, float offset, float xScale, float yScale, float integralOffset)
        {
            var scaledValue = value / yScale;
            return -(float)Math.Log(
                        Math.Pow(Math.E, xScale * (integralOffset - scaledValue))
                        - Math.Pow(Math.E, -offset * xScale)
                    ) / xScale;
        }
    }
}
