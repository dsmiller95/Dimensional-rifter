using Unity.Entities;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    [GenerateAuthoringComponent]
    public struct GrowingThingComponent : IComponentData
    {
        public float currentGrowth;
        public float growthPerSecond;
        public float finalGrowthAmount;
        public bool Grown;

        public void SetGrownAmount(float grownAmount)
        {
            currentGrowth = grownAmount;
            if (currentGrowth >= finalGrowthAmount)
            {
                currentGrowth = finalGrowthAmount;
                Grown = true;
                //errandBoard.RegisterErrandSource(this);
            }
            else
            {
                Grown = false;
                //GetComponent<SpriteRenderer>().sprite = growingSprites[growthSpriteID];
            }
        }
    }
}
