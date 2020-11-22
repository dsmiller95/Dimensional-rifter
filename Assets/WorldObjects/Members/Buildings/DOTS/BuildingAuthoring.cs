using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Storage.DOTS;
using Assets.WorldObjects.Members.Wall.DOTS;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Assets.WorldObjects.Members.Buildings.DOTS
{
    /// <summary>
    /// Building component. Authoring workflow works like this:
    ///     In the prefab, the top-level gameobject is what will exist when the building is completed
    ///     As a child of that gameObject, create another gameObject to represent what will exist as the building preview
    ///     The top-level gameObject will be disabled when it is converted to entities; only enabled again when the Building Entity is built
    ///     the building entity will be trimmed down to only inventory information when it is built, throwing away all rendering and position data.
    ///     The building entity can be used as a reference for what was used to build the building once it was built.
    /// The child building component will copy the <see cref="UniversalCoordinatePositionComponent"/> and <see cref="OffsetFromCoordinatePositionComponent"/> from the parent entity
    ///     it will also break the Transform system's Parent-child linkage, each entity will be translated independently by the <see cref="UniversalCoordinateToTransformSystem"/>
    /// </summary>
    [DisallowMultipleComponent]
    public class BuildingAuthoring : MonoBehaviour,
        IConvertGameObjectToEntity
    {
        public Resource ItemTypeRequriement;
        public float defaultResourceRequiredAmount;
        public SuppliableType supplyClassification;

        public void Convert(Entity buildingEntity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            // assuming that the parent gameobject has already been converted
            var controllerEntity = conversionSystem.GetPrimaryEntity(transform.parent.gameObject);

            var parentCoordinatePosition = dstManager.GetComponentData<UniversalCoordinatePositionComponent>(controllerEntity);
            var parentOffset = dstManager.GetComponentData<OffsetFromCoordinatePositionComponent>(controllerEntity);

            dstManager.AddComponentData(buildingEntity, parentCoordinatePosition);
            dstManager.AddComponentData(buildingEntity, parentOffset);
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

            //remove parent-child link, we don't need it. and it actually prevents the transform system from working correctly
            dstManager.RemoveComponent<Parent>(buildingEntity);
            dstManager.RemoveComponent<PreviousParent>(buildingEntity);
            dstManager.RemoveComponent<LocalToParent>(buildingEntity);
            dstManager.RemoveComponent<Child>(controllerEntity);
        }
    }
}
