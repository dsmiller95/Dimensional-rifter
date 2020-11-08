using Unity.Entities;

namespace Assets.WorldObjects.Members.Wall.DOTS
{
    /// <summary>
    /// set on any components which are being built -- they are ghosts
    /// </summary>
    public struct IsNotBuiltFlag: IComponentData
    {
    }

    public struct BuildErrandTargetComponent: IComponentData
    {
    }
}
