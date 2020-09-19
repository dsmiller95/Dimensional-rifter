using UnityEngine;

namespace Assets.WorldObjects.Members.Items
{
    [CreateAssetMenu(fileName = "SpawnableItemType", menuName = "Members/Inventory/SpawnableItem", order = 1)]
    public class ResourceItemType : MemberType
    {
        public Resource resourceType;
    }
}
