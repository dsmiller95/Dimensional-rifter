using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "SubInventory", menuName = "Members/Inventory/SupplyType", order = 1)]
    public class SupplyType : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
    }
}
