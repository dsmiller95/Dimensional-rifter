
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
                .WithNone<OffsetLayerComponent>()
                .ForEach((ref Translation translation, in UniversalCoordinatePositionComponent positionCoordinate) =>
            {
                var position = positionCoordinate.Value.ToPositionInPlane();
                translation = new Translation
                {
                    Value = new float3(position, translation.Value.z)
                };
            }).ScheduleParallel();

            Entities
                .WithChangeFilter<UniversalCoordinatePositionComponent>()
                .WithNone<OffsetFromCoordinatePositionComponent>()
                .WithAll<OffsetLayerComponent>()
                .ForEach((ref Translation translation, in UniversalCoordinatePositionComponent positionCoordinate, in OffsetLayerComponent offsetLayer) =>
                {
                    var position = positionCoordinate.Value.ToPositionInPlane();
                    translation = new Translation
                    {
                        Value = offsetLayer.ApplyPositionInOrderingLayer(position)
                    };
                }).ScheduleParallel();


            Entities
                .WithChangeFilter<UniversalCoordinatePositionComponent, OffsetFromCoordinatePositionComponent>()
                .WithAll<OffsetFromCoordinatePositionComponent>()
                .WithNone<OffsetLayerComponent>()
                .ForEach((ref Translation translation, in UniversalCoordinatePositionComponent positionCoordinate, in OffsetFromCoordinatePositionComponent offset) =>
                {
                    var position = positionCoordinate.Value.ToPositionInPlane() + offset.Value;
                    translation = new Translation
                    {
                        Value = new float3(position, translation.Value.z)
                    };
                }).ScheduleParallel();

            Entities
                //.WithChangeFilter<UniversalCoordinatePositionComponent, OffsetFromCoordinatePositionComponent>()
                .WithAll<OffsetFromCoordinatePositionComponent>()
                .WithAll<OffsetLayerComponent>()
                .ForEach((ref Translation translation, in UniversalCoordinatePositionComponent positionCoordinate, in OffsetFromCoordinatePositionComponent offset, in OffsetLayerComponent offsetLayer) =>
                {
                    var position = positionCoordinate.Value.ToPositionInPlane() + offset.Value;
                    translation = new Translation
                    {
                        Value = offsetLayer.ApplyPositionInOrderingLayer(position)
                    };
                }).ScheduleParallel();
        }

    }
}
