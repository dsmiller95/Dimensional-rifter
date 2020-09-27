using Assets.WorldObjects.Members;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    [CreateAssetMenu(fileName = "ErrandTypeRegistry", menuName = "Behaviors/ErrandTypeRegistry", order = 1)]
    public class ErrandTypeRegistry : UniqueObjectRegistryWithAccess<ErrandType>
    {
    }
}
