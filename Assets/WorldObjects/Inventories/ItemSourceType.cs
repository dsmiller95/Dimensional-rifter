using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "ItemSourceType", menuName = "Members/Inventory/ItemSourceType", order = 1)]
    public class ItemSourceType : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
    }
}
