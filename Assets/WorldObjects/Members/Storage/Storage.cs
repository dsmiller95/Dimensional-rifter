using Assets.WorldObjects.Members;
using System;
using UnityEngine;

namespace Assets.WorldObjects
{
    [Serializable]
    class StorageSaveObject
    {
        public ResourceInventorySaveData inventory;
    }
    [RequireComponent(typeof(ResourceInventory))]
    [RequireComponent(typeof(TileMapMember))]
    public class Storage : MonoBehaviour, IMemberSaveable
    {
        public MemberType GetMemberType()
        {
            return MemberType.STORAGE;
        }

        public object GetSaveObject()
        {
            return new StorageSaveObject
            {
                inventory = GetComponent<ResourceInventory>().GetSaveObject()
            };
        }

        public void SetupFromSaveObject(object save)
        {
            var saveObject = save as StorageSaveObject;
            GetComponent<ResourceInventory>().SetupFromSaveObject(saveObject.inventory);
        }
    }
}
