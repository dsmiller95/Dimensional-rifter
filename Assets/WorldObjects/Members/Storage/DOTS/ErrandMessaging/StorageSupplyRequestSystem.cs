using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Items.DOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging
{
    struct ItemAmountSourceRepresentation
    {
        public Entity itemEntity;
        public float itemAmount;
        public int indexInItemAmountBuffer;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class StorageSupplyRequestSystem : SystemBase
    {
        private EntityQuery SupplyErrandRequests;

        protected override void OnCreate()
        {
            SupplyErrandRequests = GetEntityQuery(
                ComponentType.ReadOnly<StorageSupplyErrandRequestComponent>(),
                ComponentType.ReadOnly<UniversalCoordinatePositionComponent>(),
                ComponentType.Exclude<StorageSupplyErrandResultComponent>()
                );
        }

        EntityCommandBufferSystem finishedRequestBufferSystem => World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        ConnectivityEntitySystem ConnectivitySystem => World.GetOrCreateSystem<ConnectivityEntitySystem>();
        protected override void OnUpdate()
        {
            var connectionSystem = ConnectivitySystem;
            if (SupplyErrandRequests.IsEmpty || !connectionSystem.HasRegionMaps)
            {
                return;
            }
            var supplyErrandEntities = SupplyErrandRequests.ToEntityArrayAsync(Allocator.TempJob, out var supplyEntityJob);
            var supplyErrandRequests = SupplyErrandRequests.ToComponentDataArrayAsync<StorageSupplyErrandRequestComponent>(Allocator.TempJob, out var supplyErrandJob);
            var supplyErrandPositions = SupplyErrandRequests.ToComponentDataArrayAsync<UniversalCoordinatePositionComponent>(Allocator.TempJob, out var supplyOriginPositionJob);
            var dataGrab = JobHandle.CombineDependencies(supplyEntityJob, supplyErrandJob, supplyOriginPositionJob);
            dataGrab.Complete();

            //JobHandle tempDependency = default;

            //NativeHashMap<int, Entity> availableSupplyTargets = new NativeHashMap<int, Entity>(System.Enum.GetValues(typeof(Resource)).Length, Allocator.TempJob);

            var commandBuffer = finishedRequestBufferSystem.CreateCommandBuffer();
            var regionMap = connectionSystem.Regions;

            for (var supplyIndex = 0; supplyIndex < supplyErrandRequests.Length; supplyIndex++)
            {
                var supplyRequest = supplyErrandRequests[supplyIndex];
                if (!supplyRequest.DataIsSet)
                {
                    continue;
                }
                if(!regionMap.TryGetValue(supplyErrandPositions[supplyIndex].Value, out var supplyRequestRegion))
                {
                    continue;
                }
                NativeHashMap<int, ItemAmountSourceRepresentation> availableResourceTargets = new NativeHashMap<int, ItemAmountSourceRepresentation>(System.Enum.GetValues(typeof(Resource)).Length, Allocator.TempJob);
                Entities
                    .ForEach((int entityInQueryIndex, Entity self,
                        in UniversalCoordinatePositionComponent position,
                        in ItemSourceTypeComponent itemType,
                        in ItemAmountsDataComponent amount,
                        in DynamicBuffer<ItemAmountClaimBufferData> itemAmountBuffer) =>
                    {
                        if ((itemType.SourceTypeFlag & supplyRequest.ItemSourceTypeFlags) == 0)
                        {
                            return;
                        }
                        if(!regionMap.TryGetValue(position.Value, out var itemSourceRegion) || (itemSourceRegion & supplyRequestRegion) == 0)
                        {
                            return;
                        }

                        for (var itemAmountIndex = 0; itemAmountIndex < itemAmountBuffer.Length; itemAmountIndex++)
                        {
                            var itemAmount = itemAmountBuffer[itemAmountIndex];
                            var resourceTypeID = (int)itemAmount.Type;
                            var totalClaimableAmount = itemAmount.Amount - itemAmount.TotalSubtractionClaims;
                            if (totalClaimableAmount <= 0 || availableResourceTargets.ContainsKey(resourceTypeID))
                            {
                                continue;
                            }

                            availableResourceTargets.Add(resourceTypeID, new ItemAmountSourceRepresentation
                            {
                                itemEntity = self,
                                itemAmount = itemAmount.Amount,
                                indexInItemAmountBuffer = itemAmountIndex
                            });
                        }
                    }).Run();//.Schedule(tempDependency);
                NativeList<StorageSupplyErrandResultComponent> possibleResults = new NativeList<StorageSupplyErrandResultComponent>(1, Allocator.TempJob);

                Entities
                    .ForEach((int entityInQueryIndex, Entity self,
                        in UniversalCoordinatePositionComponent position,
                        in SupplyTypeComponent storageType,
                        in ItemAmountsDataComponent itemAmountInSupplyTarget,
                        in DynamicBuffer<ItemAmountClaimBufferData> itemSupplyAmounts) =>
                    {
                        if ((storageType.SupplyTypeFlag & supplyRequest.SupplyTargetType) == 0)
                        {
                            return;
                        }
                        if (!regionMap.TryGetValue(position.Value, out var supplyTargetRegion) || (supplyTargetRegion & supplyRequestRegion) == 0)
                        {
                            return;
                        }

                        var totalAmounts = itemSupplyAmounts.TotalAmounts();
                        var remainingSpace = itemAmountInSupplyTarget.MaxCapacity - totalAmounts - itemAmountInSupplyTarget.TotalAdditionClaims;
                        if (remainingSpace <= 0)
                        {
                            return;
                        }

                        if (itemAmountInSupplyTarget.LockItemDataBufferTypes)
                        {
                            for (int indexInSupplyType = 0; indexInSupplyType < itemSupplyAmounts.Length; indexInSupplyType++)
                            {
                                var supplyAmountTypeID = (int)itemSupplyAmounts[indexInSupplyType].Type;
                                if (availableResourceTargets.TryGetValue(supplyAmountTypeID, out var resourceTypeAmount))
                                {
                                    var possibleResult = GetSupplyErrandResult(
                                        supplyAmountTypeID,
                                        resourceTypeAmount,
                                        remainingSpace,
                                        self);
                                    possibleResults.Add(possibleResult);
                                }
                            }
                        }
                        else
                        {
                            var resourceTargets = availableResourceTargets.GetEnumerator();

                            while (resourceTargets.MoveNext())
                            {
                                var resourceTypeAmount = resourceTargets.Current;
                                var possibleResult = GetSupplyErrandResult(
                                    resourceTypeAmount.Key,
                                    resourceTypeAmount.Value,
                                    remainingSpace,
                                    self);
                                possibleResults.Add(possibleResult);
                            }
                            resourceTargets.Dispose();
                        }
                    }).Run();//.Schedule(tempDependency);

                availableResourceTargets.Dispose();
                var didSetResult = false;
                foreach (var action in possibleResults)
                {
                    if (action.amountToTransfer <= 0)
                    {
                        continue;
                    }
                    // all this code is to check if modifications were made to these values?
                    //      ideally the whole job should exclusively lock these items

                    var storage = GetComponent<ItemAmountsDataComponent>(action.supplyTarget);
                    var storageItems = GetBuffer<ItemAmountClaimBufferData>(action.supplyTarget);

                    var remainingSpace = storage.MaxCapacity - storageItems.TotalAmounts() - storage.TotalAdditionClaims;
                    if (remainingSpace <= 0)
                    {
                        continue;
                    }

                    var amountToTransfer = math.min(action.amountToTransfer, remainingSpace);

                    var itemAmountBuffer = GetBuffer<ItemAmountClaimBufferData>(action.itemSource);
                    var itemIndex = itemAmountBuffer.IndexOfType(action.resourceTransferType);
                    if (itemIndex < 0)
                    {
                        continue;
                    }
                    var amount = itemAmountBuffer[itemIndex];
                    amount.TotalSubtractionClaims += amountToTransfer;
                    itemAmountBuffer[itemIndex] = amount;

                    storage.TotalAdditionClaims += amountToTransfer;
                    SetComponent(action.supplyTarget, storage);

                    commandBuffer.AddComponent(supplyErrandEntities[supplyIndex], action);
                    didSetResult = true;
                    break;
                }
                if (!didSetResult)
                {
                    commandBuffer.DestroyEntity(supplyErrandEntities[supplyIndex]);
                }
                possibleResults.Dispose();
            }
            supplyErrandPositions.Dispose();
            supplyErrandEntities.Dispose();
            supplyErrandRequests.Dispose();

            finishedRequestBufferSystem.AddJobHandleForProducer(Dependency);
        }

        private static StorageSupplyErrandResultComponent GetSupplyErrandResult(
            int resourceTypeId,
            ItemAmountSourceRepresentation resourceSourceInfo,
            float remainingSpace,
            Entity supplyTarget)
        {
            var availableAmount = resourceSourceInfo.itemAmount;

            var amountToTransfer = math.min(availableAmount, remainingSpace);

            return new StorageSupplyErrandResultComponent
            {
                itemSource = resourceSourceInfo.itemEntity,
                supplyTarget = supplyTarget,
                resourceTransferType = (Resource)resourceTypeId,
                amountToTransfer = amountToTransfer
            };
        }

    }
}
