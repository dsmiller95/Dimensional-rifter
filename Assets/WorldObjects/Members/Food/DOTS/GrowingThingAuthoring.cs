using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Food.DOTS
{
    public class GrowingThingAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Resource grownResource;
        public float resourceAmount;

        public float growthPerSecond;
        public float finalGrowthAmount;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new GrowingThingComponent
            {
                currentGrowth = 0,
                growthPerSecond = growthPerSecond,
                finalGrowthAmount = finalGrowthAmount,
                Grown = false
            });
            dstManager.AddComponentData(entity, new GrowthProductComponent
            {
                grownResource = grownResource,
                resourceAmount = resourceAmount
            });
        }
    }
}
