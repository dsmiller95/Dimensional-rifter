using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Assets.WorldObjects.SaveObjects.SaveManager
{
    public static class SerializationManager
    {
        public static bool Save(string saveName, object saveData)
        {
            var formatter = SerializationManager.GetBinaryFormatter();

            if (!Directory.Exists(Application.persistentDataPath + "/saves"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/saves");
            }

            string path = SerializationManager.GetSavePath(saveName);

            FileStream file = File.Create(path);

            formatter.Serialize(file, saveData);
            file.Close();
            return true;
        }

        private static string GetSavePath(string saveName)
        {
            return Application.persistentDataPath + "/saves/" + saveName + ".save";
        }


        public static object Load(string saveName)
        {
            var path = SerializationManager.GetSavePath(saveName);
            if (!File.Exists(path))
            {
                return null;
            }

            var formatter = SerializationManager.GetBinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);

            try
            {
                return formatter.Deserialize(file);
            }
            catch
            {
                Debug.LogError($"Failed to load file at {path}");
                return null;
            }
            finally
            {
                file.Close();
            }
        }

        private static BinaryFormatter GetBinaryFormatter()
        {
            return new BinaryFormatter();
        }
    }
}
