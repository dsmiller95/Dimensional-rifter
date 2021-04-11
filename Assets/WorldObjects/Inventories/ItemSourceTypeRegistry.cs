using Assets.WorldObjects.Members;
using Dman.ObjectSets;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "ItemSourceTypeRegistry", menuName = "Behaviors/ItemSourceTypeRegistry", order = 1)]
    public class ItemSourceTypeRegistry : UniqueObjectRegistryWithAccess<ItemSourceType>
    {
    }
}
