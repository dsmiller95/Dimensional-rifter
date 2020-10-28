using Assets.WorldObjects.DOTSMembers;
using System;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Tiling.Tilemapping.DOTSTilemap
{
    public class UniversalCoordinateToTransformSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithChangeFilter<UniversalCoordinatePosition>()
                .ForEach((ref Translation translation, ref UniversalCoordinatePosition positionCoordinate) =>
            {
                translation = new Translation
                {
                    Value = translation.Value + new float3(1, 0, 0)
                };
                // todo: use blob data of the current coordinates to update??

                //translation.Value.y += moveSpeedComponent.moveSpeed * Time.DeltaTime;
                //if (translation.Value.y > 5f)
                //{
                //    moveSpeedComponent.moveSpeed = -math.abs(moveSpeedComponent.moveSpeed);
                //}
                //if (translation.Value.y < -5f)
                //{
                //    moveSpeedComponent.moveSpeed = +math.abs(moveSpeedComponent.moveSpeed);
                //}
            }).Run();
        }

    }
}
