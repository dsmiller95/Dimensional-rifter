using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using Unity.Entities;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    [DisallowMultipleComponent]
    public class BuildingAuthoring : MonoBehaviour,
        IConvertGameObjectToEntity
    {
        public Resource ItemTypeRequriement;
        public float defaultResourceRequiredAmount;
        public SuppliableType supplyClassification;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var parentCoordinatePosition = dstManager.GetComponentData<UniversalCoordinatePositionComponent>(entity);

            var buildingEntity = dstManager.CreateEntity(
                typeof(UniversalCoordinatePositionComponent),
                typeof(IsNotBuiltFlag),
                typeof(ItemAmountsDataComponent),
                typeof(ItemAmountClaimBufferData),
                typeof(SupplyTypeComponent),
                typeof(BuildingChildComponent));

            dstManager.SetComponentData(buildingEntity, parentCoordinatePosition);

            dstManager.SetComponentData(buildingEntity, new ItemAmountsDataComponent
            {
                MaxCapacity = defaultResourceRequiredAmount,
                TotalAdditionClaims = 0f,
                LockItemDataBufferTypes = true
            });
            DynamicBuffer<ItemAmountClaimBufferData> itemAmounts = dstManager.GetBuffer<ItemAmountClaimBufferData>(buildingEntity);
            itemAmounts.Add(new ItemAmountClaimBufferData
            {
                Type = ItemTypeRequriement,
                Amount = 0f,
                TotalSubtractionClaims = 0f
            });
            dstManager.SetComponentData(buildingEntity, new SupplyTypeComponent
            {
                SupplyTypeFlag = ((uint)1) << supplyClassification.ID
            });

            dstManager.SetComponentData(buildingEntity, new BuildingChildComponent
            {
                controllerComponent = entity
            });
            dstManager.AddComponentData(entity, new BuildingParentComponent
            {
                buildingEntity = buildingEntity
            });
        }
    }
}
