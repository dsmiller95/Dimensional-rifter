using Assets.WorldObjects.Members.Items.DOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.WorldObjects.Members.Storage.DOTS.ErrandMessaging
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class StorageSupplyRequestSystem : SystemBase
    {
        private EntityQuery LooseItems;
        private EntityQuery StorageSites;

        private EntityQuery SupplyErrandRequests;

        protected override void OnCreate()
        {
            LooseItems = GetEntityQuery(
                ComponentType.ReadOnly<ItemSourceTypeComponent>(),
                ComponentType.ReadOnly<ItemAmountComponent>(),
                ComponentType.ReadOnly<LooseItemFlagComponent>(),
                ComponentType.ReadOnly<ItemSubtractClaimComponent>());

            StorageSites = GetEntityQuery(
                ComponentType.ReadOnly<SupplyTypeComponent>(),
                ComponentType.ReadOnly<StorageDataComponent>(),
                ComponentType.ReadOnly<ItemAmountClaimBufferData>()
                );

            SupplyErrandRequests = GetEntityQuery(
                ComponentType.ReadOnly<StorageSupplyErrandRequestComponent>(),
                ComponentType.Exclude<StorageSupplyErrandResultComponent>()
                );
        }

        EntityCommandBufferSystem finishedRequestBufferSystem => World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {
            //var itemAmountType = GetComponentTypeHandle<ItemAmountComponent>();
            //var looseFlag = GetComponentTypeHandle<LooseItemFlagComponent>();
            //var itemSourceType = GetComponentTypeHandle<ItemSourceTypeComponent>();
            //var itemSubtraction = GetComponentTypeHandle<ItemSubtractClaimComponent>();

            //var storageData = GetComponentTypeHandle<StorageDataComponent>();
            //var supplyType = GetComponentTypeHandle<SupplyTypeComponent>();
            //var storageAmountClaim = GetBufferTypeHandle<ItemAmountClaimBufferData>();

            //var entity = GetEntityTypeHandle();

            var supplyErrandEntities = SupplyErrandRequests.ToEntityArrayAsync(Allocator.TempJob, out var supplyEntityJob);
            var supplyErrandRequests = SupplyErrandRequests.ToComponentDataArrayAsync<StorageSupplyErrandRequestComponent>(Allocator.TempJob, out var supplyErrandJob);
            var dataGrab = JobHandle.CombineDependencies(supplyErrandJob, supplyEntityJob);
            dataGrab.Complete();

            if (supplyErrandRequests.Length <= 0)
            {
                supplyErrandEntities.Dispose();
                supplyErrandRequests.Dispose();
                return;
            }

            //JobHandle tempDependency = default;

            //var sourceChunks = LooseItems.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out var itemJob);
            //var targetChunks = StorageSites.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out var storageJob);
            //this.Dependency = JobHandle.CombineDependencies(Dependency, itemJob, storageJob);

            //NativeHashMap<int, Entity> availableSupplyTargets = new NativeHashMap<int, Entity>(System.Enum.GetValues(typeof(Resource)).Length, Allocator.TempJob);

            var commandBuffer = finishedRequestBufferSystem.CreateCommandBuffer();

            for (var supplyIndex = 0; supplyIndex < supplyErrandRequests.Length; supplyIndex++)
            {
                var supplyRequest = supplyErrandRequests[supplyIndex];
                if (!supplyRequest.DataIsSet)
                {
                    continue;
                }
                NativeHashMap<int, Entity> availableResourceTargets = new NativeHashMap<int, Entity>(System.Enum.GetValues(typeof(Resource)).Length, Allocator.TempJob);
                Entities
                    .WithAll<LooseItemFlagComponent>()
                    .ForEach((int entityInQueryIndex, Entity self,
                        in ItemSourceTypeComponent itemType,
                        in ItemAmountComponent amount,
                        in ItemSubtractClaimComponent subtractClaim) =>
                    {
                        if ((itemType.SourceTypeFlag & supplyRequest.ItemSourceTypeFlags) == 0)
                        {
                            return;
                        }
                        var resourceType = (int)amount.resourceType; // todo: check if there's a subtractable amount
                        var resourceAmount = amount.resourceAmount - subtractClaim.TotalAllocatedSubtractions;

                        if (resourceAmount <= 0 || availableResourceTargets.ContainsKey(resourceType))
                        {
                            return;
                        }
                        availableResourceTargets.Add(resourceType, self);
                    }).Run();//.Schedule(tempDependency);
                NativeList<StorageSupplyErrandResultComponent> possibleResults = new NativeList<StorageSupplyErrandResultComponent>(1, Allocator.TempJob);
                var availableKeys = availableResourceTargets.GetKeyArray(Allocator.TempJob);
                Entities
                    .WithDisposeOnCompletion(availableResourceTargets)
                    .ForEach((int entityInQueryIndex, Entity self,
                        in SupplyTypeComponent storageType,
                        in StorageDataComponent storageInfo,
                        in DynamicBuffer<ItemAmountClaimBufferData> storageAmounts) =>
                    {
                        if ((storageType.SupplyTypeFlag & supplyRequest.SupplyTargetType) == 0)
                        {
                            return;
                        }
                        var totalAmounts = storageAmounts.TotalAmounts();
                        var remainingSpace = storageInfo.MaxCapacity - totalAmounts - storageInfo.TotalAdditionClaims;
                        if (remainingSpace <= 0)
                        {
                            return;
                        }

                        foreach (var resourceType in availableKeys)
                        {
                            if (availableResourceTargets.TryGetValue(resourceType, out var itemEntity))
                            {
                                var itemData = GetComponent<ItemAmountComponent>(itemEntity);
                                var itemSubtract = GetComponent<ItemSubtractClaimComponent>(itemEntity);
                                var availableAmount = itemData.resourceAmount - itemSubtract.TotalAllocatedSubtractions;

                                var amountToTransfer = math.min(availableAmount, remainingSpace);

                                var result = new StorageSupplyErrandResultComponent
                                {
                                    itemSource = itemEntity,
                                    supplyTarget = self,
                                    resourceTransferType = (Resource)resourceType,
                                    amountToTransfer = amountToTransfer
                                };
                                possibleResults.Add(result);
                            }
                        }
                    }).Run();//.Schedule(tempDependency);
                //tempDependency.Complete();
                availableResourceTargets.Dispose();
                availableKeys.Dispose();
                var didSetResult = false;
                foreach (var action in possibleResults)
                {
                    if (action.amountToTransfer <= 0)
                    {
                        continue;
                    }
                    // all this code is to check if modifications were made to these values?
                    //      ideally the whole job should exclusively lock these items
                    var amount = GetComponent<ItemAmountComponent>(action.itemSource);
                    var subtract = GetComponent<ItemSubtractClaimComponent>(action.itemSource);
                    var remainingAmount = amount.resourceAmount - subtract.TotalAllocatedSubtractions;
                    if (remainingAmount <= 0)
                    {
                        continue;
                    }

                    var storage = GetComponent<StorageDataComponent>(action.supplyTarget);
                    var storageItems = GetBuffer<ItemAmountClaimBufferData>(action.supplyTarget);

                    var remainingSpace = storage.MaxCapacity - storageItems.TotalAmounts() - storage.TotalAdditionClaims;
                    if (remainingSpace <= 0)
                    {
                        continue;
                    }

                    var amountToTransfer = math.min(action.amountToTransfer, math.min(remainingAmount, remainingSpace));
                    subtract.TotalAllocatedSubtractions += amountToTransfer;
                    SetComponent(action.itemSource, subtract);

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
            supplyErrandEntities.Dispose();
            supplyErrandRequests.Dispose();

            finishedRequestBufferSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
