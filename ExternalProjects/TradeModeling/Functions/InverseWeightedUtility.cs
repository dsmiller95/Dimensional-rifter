using System;
using System.Linq;

namespace TradeModeling.Functions
{
    [Serializable]
    public struct WeightedRegion
    {
        public WeightedRegion(int beginning, float weight)
        {
            RegionBeginning = beginning;
            RegionWeight = weight;
        }
        public int RegionBeginning;
        public float RegionWeight;
    }

    public class InverseWeightedUtility : IIncrementalFunction
    {
        private WeightedRegion[] regions;
        private float offset;

        public InverseWeightedUtility(WeightedRegion[] regions, float offset = 1)
        {
            this.regions = regions;
            this.offset = offset;
        }

        /// <summary>
        /// Calculate the utility of getting one more thing
        /// // TODO: convert to using a continuous, integratable function
        /// </summary>
        /// <param name="currentInventory">the current amount of the Item</param>
        /// <returns>The additional utility from gaining one more item</returns>
        public float GetIncrementalValue(float currentInventory, float increment)
        {
            if (increment < 0)
            {
                return -GetIncrementalValue(currentInventory + increment, -increment);
            }
            if ((increment % 1) > 0.0001)
            {
                throw new NotImplementedException($"Cannot calculate incremental utility in increments other than 1. attempted {increment}");
            }
            var currentRegion = RegionAt(currentInventory);
            var onePlusIncrement = currentRegion.RegionWeight * BaseUtility(currentInventory);
            if (increment > 1)
            {
                onePlusIncrement += GetIncrementalValue(currentInventory + 1, increment - 1);
            }
            return onePlusIncrement;
        }

        public float GetNetValue(float startPoint)
        {
            int realStartPoint = (int)startPoint;
            float totalUtility = 0;
            for (int i = realStartPoint - 1; i >= 0; i--)
            {
                totalUtility += GetIncrementalValue(i, 1);
            }
            return totalUtility;
        }

        public float GetPointFromNetExtraValueFromPoint(float extraValue, float startPoint)
        {
            throw new NotImplementedException();
        }

        public float GetPointFromNetValue(float value)
        {
            throw new NotImplementedException();
        }

        private float BaseUtility(float inventory)
        {
            return 1f / (inventory + offset);
        }

        private WeightedRegion RegionAt(float inventory)
        {
            try
            {
                return regions
                    .Where(region => region.RegionBeginning <= inventory)
                    .Last();
            }
            catch
            {
                throw new Exception($"no defined region at {inventory}");
            }
        }
    }
}
