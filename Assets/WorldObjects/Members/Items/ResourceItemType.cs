using Assets.WorldObjects.SaveObjects;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects.Members.Items
{
    [CreateAssetMenu(fileName = "SpawnableItemType", menuName = "Members/Inventory/SpawnableItem", order = 1)]
    public class ResourceItemType : MemberType
    {
        public Resource resourceType;
        public float MapSpawnAmount;

        public override InMemberObjectData[] InstantiateNewSaveObject()
        {
            var baseArray = base.InstantiateNewSaveObject();
            var itemData = new InMemberObjectData
            {
                data = new ItemSaveObject
                {
                    remainingResourceAmount = new Scripts.ResourceManagement.LimitedResourcePoolSaveObject
                    {
                        currentAmount = MapSpawnAmount,
                        maxCapacity = ItemController.ItemMaxCapacity
                    }
                },
                identifierInMember = ItemController.SaveDataIndentifier
            };
            return baseArray.Append(itemData).ToArray();
        }
    }
}
