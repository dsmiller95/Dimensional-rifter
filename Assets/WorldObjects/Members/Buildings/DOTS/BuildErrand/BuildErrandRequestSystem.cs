using Assets.Scripts.DOTS.ErrandClaims;
using Assets.Tiling;
using Assets.WorldObjects.DOTSMembers;
using Assets.WorldObjects.Members.Wall.DOTS;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Buildings.DOTS.BuildErrand
{
    public struct BuildErrandRequestComponent : IComponentData
    {
        public bool DataIsSet;
    }

    public struct BuildErrandResultComponent : IComponentData
    {
        public Entity constructTarget;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class DeconstructErrandRequestSystem : ErrandRequestSystem<BuildErrandRequestComponent, BuildErrandResultComponent>
    {
        protected override bool PreCheckRequest(in BuildErrandRequestComponent requestData)
        {
            return requestData.DataIsSet;
        }
        protected override void CheckJob(
            Entity requestEntity,
            BuildErrandRequestComponent requestData,
            uint requestRegion,
            NativeHashMap<UniversalCoordinate, uint> regionMap,
            EntityCommandBuffer commandBuffer)
        {
            var didSetResult = new NativeArray<bool>(new[] { false }, Allocator.TempJob);
            Entities
                .WithReadOnly(regionMap)
                .WithAll<IsNotBuiltFlag>()
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
                    commandBuffer.AddComponent(requestEntity, new BuildErrandResultComponent
                    {
                        constructTarget = self
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
