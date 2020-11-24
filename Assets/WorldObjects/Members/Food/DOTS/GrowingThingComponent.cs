using Unity.Entities;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    public struct GrowingThingComponent : IComponentData
    {
        public float currentGrowth;
        public float growthPerSecond;
        public float finalGrowthAmount;
        public bool Grown;

        public bool SetGrownAmount(float grownAmount)
        {
            currentGrowth = grownAmount;
            if (currentGrowth >= finalGrowthAmount)
            {
                currentGrowth = finalGrowthAmount;
                Grown = true;
            }
            else
            {
                Grown = false;
            }
            return Grown;
        }

        public bool AfterHarvested()
        {
            var result = Grown;
            SetGrownAmount(0);
            return result;
        }
    }
}
