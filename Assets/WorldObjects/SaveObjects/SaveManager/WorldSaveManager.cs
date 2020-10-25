using UnityEngine;

namespace Assets.WorldObjects.SaveObjects.SaveManager
{
    public class WorldSaveManager : MonoBehaviour
    {
        public GameObject worldPrefab;
        public GameObject worldObject;

        public void Start()
        {
            Load();
        }

        public void Save()
        {
            var saveDataObject = worldObject.GetComponent<ISaveable<WorldSaveObject>>();
            var data = saveDataObject.GetSaveObject();
            SaveSystemHooks.TriggerPreSave();
            SerializationManager.Save(SaveContext.instance.saveFile, data);
            SaveSystemHooks.TriggerPostSave();
        }

        public void Load()
        {
            SaveSystemHooks.TriggerPreLoad();
            var loadedData = SerializationManager.Load(SaveContext.instance.saveFile);
            if (loadedData != null && loadedData is WorldSaveObject worldSaveData)
            {
                if (worldObject != null)
                {
                    Destroy(worldObject);
                }

                worldObject = Instantiate(worldPrefab, transform);
                var saveDataObject = worldObject.GetComponent<ISaveable<WorldSaveObject>>();
                saveDataObject.SetupFromSaveObject(worldSaveData);
            }
            SaveSystemHooks.TriggerPostLoad();
        }
    }
}
