namespace TradeModeling.Functions
{

    public interface IIncrementalFunction
    {
        /// <summary>
        /// Calculate the gain/loss of moving along the function by increment
        /// </summary>
        /// <returns>The gain/loss from moving across the function by increment</returns>
        float GetIncrementalValue(float startPoint, float increment);

        /// <summary>
        /// Calculate the total value of this amount. Equivalent to the incremental value from 0 to <paramref name="startPoint"/>
        /// </summary>
        /// <returns>The gain/loss from moving across the function by increment</returns>
        float GetNetValue(float startPoint);


        float GetPointFromNetValue(float value);

        float GetPointFromNetExtraValueFromPoint(float extraValue, float startPoint);

        /// <summary>
        /// Gets the instantaneous rate of change at the given point
        /// </summary>
        /// <param name="startPoint"></param>
        /// <returns>the instantaneous rate of change at this point</returns>
        // float GetDerivativeAtPoint(float startPoint);
    }
}
