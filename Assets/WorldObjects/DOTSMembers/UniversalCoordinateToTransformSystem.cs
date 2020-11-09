
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.WorldObjects.DOTSMembers
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class UniversalCoordinateToTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<UniversalCoordinatePositionComponent>()
                .WithNone<OffsetFromCoordinatePositionComponent>()
                .ForEach((ref Translation translation, in UniversalCoordinatePositionComponent positionCoordinate) =>
            {
                var position = positionCoordinate.coordinate.ToPositionInPlane();

                translation = new Translation
                {
                    Value = new float3(position, translation.Value.z)
                };
            }).ScheduleParallel();


            Entities
                //.WithChangeFilter<UniversalCoordinatePositionComponent, OffsetFromCoordinatePositionComponent>()
                .ForEach((ref Translation translation, in UniversalCoordinatePositionComponent positionCoordinate, in OffsetFromCoordinatePositionComponent offset) =>
                {
                    var position = positionCoordinate.coordinate.ToPositionInPlane() + offset.Value;

                    translation = new Translation
                    {
                        Value = new float3(position, translation.Value.z)
                    };
                }).ScheduleParallel();
        }

    }
}
