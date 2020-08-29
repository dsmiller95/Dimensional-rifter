using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "SupplyableType", menuName = "Members/Inventory/SupplyableType", order = 1)]
    public class SuppliableType : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
    }
}
