using Assets.Tiling;
using Unity.Entities;
using Unity.Transforms;
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

        private void SpawnItemTransferAnim(Vector3 source, Vector3 target, Transform parent = null)
        {
            GameObject newParticleSystem;
            if(parent == null)
            {
                newParticleSystem = GameObject.Instantiate(itemTransferParticleSystemPrefab);
            }
            else
            {
                newParticleSystem = GameObject.Instantiate(itemTransferParticleSystemPrefab, parent);
            }
            newParticleSystem.transform.position = new Vector3(
                    source.x,
                    source.y,
                    newParticleSystem.transform.position.z);

            newParticleSystem.transform.LookAt(
                new Vector3(target.x, target.y, newParticleSystem.transform.position.z),
                Vector3.forward);
        }


        public static void ShowItemTransferAnimation(GameObject itemSource, GameObject itemTarget)
        {
            Instance.SpawnItemTransferAnim(itemSource.transform.position, itemTarget.transform.position, itemTarget.transform.parent);
        }
        public static void ShowItemTransferAnimation(GameObject itemSource, Translation itemTarget)
        {
            Instance.SpawnItemTransferAnim(itemSource.transform.position, itemTarget.Value, itemSource.transform.parent);
        }
        public static void ShowItemTransferAnimation(Translation itemSource, GameObject itemTarget)
        {
            Instance.SpawnItemTransferAnim(itemSource.Value, itemTarget.transform.position, itemTarget.transform.parent);
        }
        public static void ShowItemTransferAnimation(Translation itemSource, Translation itemTarget)
        {
            Instance.SpawnItemTransferAnim(itemSource.Value, itemTarget.Value);
        }
    }
}
