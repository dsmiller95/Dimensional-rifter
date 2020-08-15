using UnityEngine;

namespace Assets.WorldObjects.SaveObjects.SaveManager
{
    public class WorldSaveManager : MonoBehaviour
    {
        public string saveFileName;

        public GameObject worldPrefab;
        public GameObject worldObject;

        public void Save()
        {
            var saveDataObject = worldObject.GetComponent<ISaveable<World>>();
            var data = saveDataObject.GetSaveObject();
            SerializationManager.Save(saveFileName, data);
        }

        public void Load()
        {
            var loadedData = SerializationManager.Load(saveFileName);
            if (loadedData != null && loadedData is World worldSaveData)
            {
                Destroy(worldObject);

                worldObject = Instantiate(worldPrefab, transform);
                var saveDataObject = worldObject.GetComponent<ISaveable<World>>();
                saveDataObject.SetupFromSaveObject(worldSaveData);
            }
        }
    }
}
