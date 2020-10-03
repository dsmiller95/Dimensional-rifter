using UnityEngine;

namespace Assets.UI.ItemTransferAnimations
{

    public class ItemTransferParticleProvider : MonoBehaviour
    {
        public static ItemTransferParticleProvider Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Item transfer animation provider already registered");
            }
            Instance = this;
        }

        public float ItemTransferAnimationTime = 0.4f;
        public GameObject itemTransferParticleSystemPrefab;

        private void SpawnItemTransferAdmin(GameObject source, GameObject target)
        {
            var newParticleSystem = GameObject.Instantiate(itemTransferParticleSystemPrefab, target.transform.parent);
            newParticleSystem.transform.position = new Vector3(
                    source.transform.position.x,
                    source.transform.position.y,
                    newParticleSystem.transform.position.z);

            newParticleSystem.transform.LookAt(
                new Vector3(target.transform.position.x, target.transform.position.y, newParticleSystem.transform.position.z),
                Vector3.up);
        }

        public static void ShowItemTransferAnimation(GameObject itemSource, GameObject itemTarget)
        {
            Instance.SpawnItemTransferAdmin(itemSource, itemTarget);
        }
    }
}
