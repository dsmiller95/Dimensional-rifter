using Assets.Scripts.DOTS.ErrandClaims;
using Assets.Tiling;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Food.DOTS.GrowingThingErrand
{
    public struct GrowingHarvestErrandRequestComponent : IComponentData
    {
        public bool DataIsSet;
    }

    public struct GrowingHarvestErrandResultComponent : IComponentData
    {
        public Entity harvestTarget;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class GrowingHarvestErrandRequestSystem : ErrandRequestSystem<GrowingHarvestErrandRequestComponent, GrowingHarvestErrandResultComponent>
    {
        protected override bool PreCheckRequest(in GrowingHarvestErrandRequestComponent requestData)
        {
            return requestData.DataIsSet;
        }
        protected override void CheckJob(
            Entity requestEntity,
            GrowingHarvestErrandRequestComponent requestData,
            uint requestRegion,
            NativeHashMap<UniversalCoordinate, uint> regionMap,
            EntityCommandBuffer commandBuffer)
        {
            var didSetResult = new NativeArray<bool>(new[] { false }, Allocator.TempJob);
            Entities
                .WithReadOnly(regionMap)
                .WithAll<GrowingThingComponent>()
                .ForEach((int entityInQueryIndex, Entity self,
                    ref ErrandClaimComponent errandClaimed,
                    in UniversalCoordinatePositionComponent position) =>
                {
                    if (didSetResult[0] || errandClaimed.Claimed)
                    {
                        return;
                    }
                    if (!regionMap.TryGetValue(position.Value, out var targetRegion) || (targetRegion & requestRegion) == 0)
                    {
                        return;
                    }
                    errandClaimed.Claimed = true;
                    commandBuffer.AddComponent(requestEntity, new GrowingHarvestErrandResultComponent
                    {
                        harvestTarget = self
                    });
                    didSetResult[0] = true;
                }).Schedule();
            Job.WithBurst().WithDisposeOnCompletion(didSetResult).WithCode(() =>
            {
                if (!didSetResult[0])
                    commandBuffer.DestroyEntity(requestEntity);
            }).Schedule();
        }
    }
}
