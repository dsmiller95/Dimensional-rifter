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
        private static StorageSaveObject GenerateNewSaveObject()
        {
            return new StorageSaveObject
            {
                inventory = ResourceInventory.GenerateEmptySaveObject()
            };
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
            if (saveObject == null)
            {
                saveObject = Storage.GenerateNewSaveObject();
            }
            GetComponent<ResourceInventory>().SetupFromSaveObject(saveObject.inventory);
        }
    }
}
