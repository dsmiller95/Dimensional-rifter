using Assets.WorldObjects.Members;
using Dman.ObjectSets;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "SupplyableType", menuName = "Members/Inventory/SupplyableType", order = 1)]
    public class SuppliableType : IDableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
    }
}
