using Assets.Tiling;
using Assets.Tiling.SquareCoords;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.WorldObjects.DOTSMembers
{
    [GenerateAuthoringComponent]
    public struct RandomProviderComponent : IComponentData
    {
        public Random value;
    }
}
