using Assets.WorldObjects.DOTSMembers;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Tiling.Tilemapping.DOTSTilemap
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public class UniversalCoordinateToTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<UniversalCoordinatePosition>()
                .ForEach((ref Translation translation, ref UniversalCoordinatePosition positionCoordinate) =>
            {
                var position = positionCoordinate.coordinate.ToPositionInPlane();

                translation = new Translation
                {
                    Value = new float3(position, translation.Value.z)
                };
            }).Run();
        }

    }
}
