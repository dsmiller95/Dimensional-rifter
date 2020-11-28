using Assets.Scripts.DOTS.ErrandClaims;
using Assets.Tiling;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Buildings.DOTS.DeconstructErrand
{
    public struct DeconstructErrandRequestComponent : IComponentData
    {
        public bool DataIsSet;
    }

    public struct DeconstructErrandResultComponent : IComponentData
    {
        public Entity deconstructTarget;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class DeconstructErrandRequestSystem : ErrandRequestSystem<DeconstructErrandRequestComponent, DeconstructErrandResultComponent>
    {
        protected override bool PreCheckRequest(in DeconstructErrandRequestComponent requestData)
        {
            return requestData.DataIsSet;
        }
        protected override void CheckJob(
            Entity requestEntity,
            DeconstructErrandRequestComponent requestData,
            uint requestRegion,
            NativeHashMap<UniversalCoordinate, uint> regionMap,
            EntityCommandBuffer commandBuffer)
        {
            var didSetResult = new NativeArray<bool>(new[] { false }, Allocator.TempJob);
            Entities
                .WithReadOnly(regionMap)
                .ForEach((int entityInQueryIndex, Entity self,
                    ref DeconstructBuildingClaimComponent errandClaimed,
                    in UniversalCoordinatePositionComponent position) =>
                {
                    if (didSetResult[0] || errandClaimed.DeconstructClaimed)
                    {
                        return;
                    }
                    if (!regionMap.TryGetValue(position.Value, out var itemSourceRegion) || (itemSourceRegion & requestRegion) == 0)
                    {
                        return;
                    }
                    errandClaimed.DeconstructClaimed = true;
                    commandBuffer.AddComponent(requestEntity, new DeconstructErrandResultComponent
                    {
                        deconstructTarget = self
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
