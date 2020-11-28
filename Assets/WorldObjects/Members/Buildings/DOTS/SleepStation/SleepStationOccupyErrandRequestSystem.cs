using Assets.Scripts.DOTS.ErrandClaims;
using Assets.Tiling;
using Assets.Tiling.Tilemapping.RegionConnectivitySystem;
using Assets.WorldObjects.DOTSMembers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.WorldObjects.Members.Buildings.DOTS.SleepStation
{
    public struct SleepStationOccupyRequestComponent : IComponentData
    {
        public bool DataIsSet;
    }

    public struct SleepStationOccupyErrandResultComponent : IComponentData
    {
        public Entity sleepTarget;
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class SleepStationOccupyErrandRequestSystem : ErrandRequestSystem<SleepStationOccupyRequestComponent, SleepStationOccupyErrandResultComponent>
    {
        protected override bool PreCheckRequest(in SleepStationOccupyRequestComponent requestData)
        {
            return requestData.DataIsSet;
        }

        protected override void CheckJob(
            Entity requestEntity,
            SleepStationOccupyRequestComponent requestData,
            uint requestRegion,
            NativeHashMap<UniversalCoordinate, uint> regionMap,
            EntityCommandBuffer commandBuffer)
        {
            var didSetResult = new NativeArray<bool>(new[] { false }, Allocator.TempJob);
            Entities
                .WithReadOnly(regionMap)
                .WithNone<DeconstructBuildingClaimComponent>()
                .ForEach((int entityInQueryIndex, Entity self,
                    ref ErrandClaimComponent errandClaimed,
                    in SleepStationOccupiedComponent sleepStation,
                    in UniversalCoordinatePositionComponent position) =>
                {
                    if (didSetResult[0] || errandClaimed.Claimed || sleepStation.Occupied)
                    {
                        return;
                    }
                    if (!regionMap.TryGetValue(position.Value, out var itemSourceRegion) || (itemSourceRegion & requestRegion) == 0)
                    {
                        return;
                    }
                    errandClaimed.Claimed = true;
                    commandBuffer.AddComponent(requestEntity, new SleepStationOccupyErrandResultComponent
                    {
                        sleepTarget = self
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
