using Assets.WorldObjects.Members.Items.DOTS;
using ECS_SpriteSheetAnimation;
using System.Collections.Generic;
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
                ComponentType.ReadOnly<ItemAmountClaimBufferData>());
        }

        EntityCommandBufferSystem finishedRequestBufferSystem => World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {

            var itemAmountType = GetComponentTypeHandle<ItemAmountComponent>();
            var looseFlag = GetComponentTypeHandle<LooseItemFlagComponent>();
            var itemSourceType = GetComponentTypeHandle<ItemSourceTypeComponent>();
            var itemSubtraction = GetComponentTypeHandle<ItemSubtractClaimComponent>();

            var storageData = GetComponentTypeHandle<StorageDataComponent>();
            var supplyType = GetComponentTypeHandle<SupplyTypeComponent>();
            var storageAmountClaim = GetBufferTypeHandle<ItemAmountClaimBufferData>();

            var entity = GetEntityTypeHandle();

            var itemChunks = LooseItems.CreateArchetypeChunkArray(Allocator.TempJob);
            var storageChunks = StorageSites.CreateArchetypeChunkArray(Allocator.TempJob);

            var commandBuffer = finishedRequestBufferSystem.CreateCommandBuffer().AsParallelWriter();

            var random = new Random((uint)(Time.ElapsedTime * 10000));

            Entities
                .WithNone<StorageSupplyErrandResultComponent>()
                .WithReadOnly(entity)
                // These are only necessary when using these types to query for their actual data inside the lambda
                //.WithReadOnly(itemAmountType).WithReadOnly(looseFlag).WithReadOnly(itemSubtraction)
                //.WithReadOnly(storageData).WithReadOnly(storageAmountClaim)
                // todo: do I have to say I'm reading and writing to itemSubtraction and storageAmountClaim?. for now I just write to a buffer
                // but there must be some way to make sure multiple conflicting modifications don't get written to the buffer
                .WithReadOnly(itemSourceType).WithReadOnly(supplyType)
                .WithDisposeOnCompletion(itemChunks).WithDisposeOnCompletion(storageChunks)
                .ForEach((int entityInQueryIndex, Entity self, in StorageSupplyErrandRequestComponent storageRequest) =>
                {
                    if (!storageRequest.DataIsSet)
                    {
                        return;
                    }
                    StorageSupplyErrandResultComponent result = new StorageSupplyErrandResultComponent
                    {
                        amountToTransfer = random.NextInt(0, 3),
                        resourceTransferType = Resource.FOOD
                    };

                    bool foundItemSource = false;
                    for (int itemChunkIndex = 0; itemChunkIndex < itemChunks.Length && !foundItemSource; itemChunkIndex++)
                    {
                        var chunk = itemChunks[itemChunkIndex];
                        var itemSourceTypeData = chunk.GetNativeArray(itemSourceType);
                        var itemEntities = chunk.GetNativeArray(entity);
                        for (int indexInChunk = 0; indexInChunk < itemSourceTypeData.Length && !foundItemSource; indexInChunk++)
                        {
                            var sourceTypeDatum = itemSourceTypeData[indexInChunk];
                            if((sourceTypeDatum.SourceTypeFlag & storageRequest.ItemSourceTypeFlags) != 0)
                            {
                                result.itemSource = itemEntities[indexInChunk];
                                foundItemSource = true;
                            }
                        }
                    }
                    if (!foundItemSource)
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex, self);
                        return;
                    }

                    bool foundSuppliable = false;
                    for (int itemChunkIndex = 0; itemChunkIndex < storageChunks.Length && !foundSuppliable; itemChunkIndex++)
                    {
                        var chunk = storageChunks[itemChunkIndex];
                        var supplyTypeData = chunk.GetNativeArray(supplyType);
                        var itemEntities = chunk.GetNativeArray(entity);
                        for (int indexInChunk = 0; indexInChunk < supplyTypeData.Length && !foundSuppliable; indexInChunk++)
                        {
                            var supplyTypeDatum = supplyTypeData[indexInChunk];
                            if ((supplyTypeDatum.SupplyTypeFlag & storageRequest.SupplyTargetType) != 0)
                            {
                                result.supplyTarget = itemEntities[indexInChunk];
                                foundSuppliable = true;
                            }
                        }
                    }
                    if (!foundSuppliable)
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex, self);
                        return;
                    }
                    commandBuffer.AddComponent(entityInQueryIndex, self, result);
                }).Schedule();


            finishedRequestBufferSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
