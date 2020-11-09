using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Items.DOTS;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using System.Collections.Generic;
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

        public GameObject controllerGameObject;

        public void Convert(Entity buildingEntity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // assuming that the parent gameobject has already been converted
            var controllerEntity = conversionSystem.GetPrimaryEntity(this.transform.parent.gameObject);

            var parentCoordinatePosition = dstManager.GetComponentData<UniversalCoordinatePositionComponent>(controllerEntity);

            dstManager.AddComponentData(buildingEntity, parentCoordinatePosition);
            dstManager.AddComponentData(buildingEntity, new IsNotBuiltFlag());

            dstManager.AddComponentData(buildingEntity, new ItemAmountsDataComponent
            {
                MaxCapacity = defaultResourceRequiredAmount,
                TotalAdditionClaims = 0f,
                LockItemDataBufferTypes = true
            });
            DynamicBuffer<ItemAmountClaimBufferData> itemAmounts = dstManager.AddBuffer<ItemAmountClaimBufferData>(buildingEntity);
            itemAmounts.Add(new ItemAmountClaimBufferData
            {
                Type = ItemTypeRequriement,
                Amount = 0f,
                TotalSubtractionClaims = 0f
            });
            dstManager.AddComponentData(buildingEntity, new SupplyTypeComponent
            {
                SupplyTypeFlag = ((uint)1) << supplyClassification.ID
            });

            dstManager.AddComponentData(buildingEntity, new BuildingChildComponent
            {
                controllerComponent = controllerEntity
            });
            dstManager.AddComponentData(controllerEntity, new BuildingParentComponent
            {
                buildingEntity = buildingEntity
            });
            dstManager.AddComponent<Disabled>(controllerEntity);
        }
    }
}
