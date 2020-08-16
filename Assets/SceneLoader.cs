using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets
{
    public class SceneLoader : MonoBehaviour
    {
        public string SceneToRestore;

        public void LoadScene()
        {
            SceneManager.LoadScene(SceneToRestore);
        }
    }
}
