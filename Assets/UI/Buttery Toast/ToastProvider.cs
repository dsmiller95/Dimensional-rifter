using TMPro;
using UnityEngine;

namespace Assets.UI.Buttery_Toast
{
    public class ToastProvider : MonoBehaviour
    {
        public static ToastProvider Instance { get; private set; }

        public GameObject toastPrefab;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Toast provider already registered");
            }
            Instance = this;
        }

        private void SpawnToast(string toastMessage, Vector3 toastworldPosition, Transform toastParent)
        {
            var newToast = GameObject.Instantiate(toastPrefab, toastParent);
            newToast.transform.position = new Vector3(toastworldPosition.x, toastworldPosition.y + .6f, newToast.transform.position.z);
            var text = newToast.GetComponentInChildren<TextMeshProUGUI>();
            text.text = toastMessage;
        }

        public static void ShowToast(string toastMessage, Vector3 toastworldPosition, Transform toastParent)
        {
            Instance.SpawnToast(toastMessage, toastworldPosition, toastParent);
        }
        public static void ShowToast(string toastMessage, GameObject toastSource)
        {
            Instance.SpawnToast(toastMessage, toastSource.transform.position, toastSource.transform.parent);
        }
    }
}
