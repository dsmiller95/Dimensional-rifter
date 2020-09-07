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
            SerializationManager.Save(SaveContext.instance.saveFile, data);
        }

        public void Load()
        {
            var loadedData = SerializationManager.Load(SaveContext.instance.saveFile);
            if (loadedData != null && loadedData is WorldSaveObject worldSaveData)
            {
                Destroy(worldObject);

                worldObject = Instantiate(worldPrefab, transform);
                var saveDataObject = worldObject.GetComponent<ISaveable<WorldSaveObject>>();
                saveDataObject.SetupFromSaveObject(worldSaveData);
            }
        }
    }
}
