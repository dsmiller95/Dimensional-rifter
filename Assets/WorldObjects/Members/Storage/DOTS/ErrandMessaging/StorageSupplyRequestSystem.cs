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
                ComponentType.ReadOnly<ItemAmountComponent>(),
                ComponentType.ReadOnly<LooseItemFlagComponent>(),
                ComponentType.ReadOnly<ItemSubtractClaimComponent>());

            StorageSites = GetEntityQuery(
                ComponentType.ReadOnly<StorageDataComponent>(),
                ComponentType.ReadOnly<ItemAmountClaimBufferData>());
        }

        EntityCommandBufferSystem finishedRequestBufferSystem => World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        protected override void OnUpdate()
        {

            var itemAmountType = GetComponentTypeHandle<ItemAmountComponent>();
            var looseFlag = GetComponentTypeHandle<LooseItemFlagComponent>();
            var itemSubtraction = GetComponentTypeHandle<ItemSubtractClaimComponent>();
            var storageData = GetComponentTypeHandle<StorageDataComponent>();
            var storageAmountClaim = GetBufferTypeHandle<ItemAmountClaimBufferData>();

            var entity = GetEntityTypeHandle();

            //var entityType = GetComponentTypeHandle<Entity>();

            var itemChunks = LooseItems.CreateArchetypeChunkArray(Allocator.TempJob);
            var storageChunks = StorageSites.CreateArchetypeChunkArray(Allocator.TempJob);

            var commandBuffer = finishedRequestBufferSystem.CreateCommandBuffer().AsParallelWriter();

            var random = new Random((uint)(Time.ElapsedTime * 10000));

            NativeArray<Entity> foundSource = new NativeArray<Entity>(1, Allocator.TempJob);
            NativeArray<Entity> foundTarget = new NativeArray<Entity>(1, Allocator.TempJob);

            Entities
                .WithNone<StorageSupplyErrandResultComponent>()
                .WithReadOnly(entity)
                // These are only necessary when using these types to query for their actual data inside the lambda
                //.WithReadOnly(itemAmountType).WithReadOnly(looseFlag).WithReadOnly(itemSubtraction)
                //.WithReadOnly(storageData).WithReadOnly(storageAmountClaim)
                // todo: do I have to say I'm reading and writing to itemSubtraction and storageAmountClaim?. for now I just write to a buffer
                // but there must be some way to make sure multiple conflicting modifications don't get written to the buffer
                .WithDisposeOnCompletion(itemChunks).WithDisposeOnCompletion(storageChunks)
                .WithDisposeOnCompletion(foundSource).WithDisposeOnCompletion(foundTarget)
                .ForEach((int entityInQueryIndex, Entity self, in StorageSupplyErrandRequestComponent storageRequest) =>
                {
                    StorageSupplyErrandResultComponent result = new StorageSupplyErrandResultComponent
                    {
                        amountToTransfer = random.NextInt(0, 3),
                        resourceTransferType = Resource.FOOD
                    };
                    if (foundSource[0] == Entity.Null)
                    {
                        if (itemChunks.Length > 0)
                        {
                            var firstchunk = itemChunks[0];
                            var items = firstchunk.GetNativeArray(entity);
                            if (items.Length > 0)
                            {
                                var itemSelection = random.NextInt(0, items.Length);
                                foundSource[0] = items[itemSelection];
                            }
                        }
                    }
                    if (foundTarget[0] == Entity.Null)
                    {
                        if (storageChunks.Length > 0)
                        {
                            var firstchunk = storageChunks[0];
                            var storages = firstchunk.GetNativeArray(entity);
                            if (storages.Length > 0)
                            {
                                var itemSelection = random.NextInt(0, storages.Length);
                                foundTarget[0] = storages[itemSelection];
                            }
                        }
                    }
                    if (foundSource[0] != Entity.Null && foundTarget[0] != Entity.Null)
                    {
                        result.itemSource = foundSource[0];
                        result.supplyTarget = foundTarget[0];
                        commandBuffer.AddComponent(entityInQueryIndex, self, result);
                    }

                }).Schedule();


            finishedRequestBufferSystem.AddJobHandleForProducer(Dependency);
        }

    }
}
