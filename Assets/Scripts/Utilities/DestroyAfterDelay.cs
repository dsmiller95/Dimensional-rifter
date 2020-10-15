using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class DestroyAfterDelay : MonoBehaviour
    {
        public float DelayTime = 1f;

        private float CreateTime;
        void Start()
        {
            CreateTime = Time.time;
        }

        void Update()
        {
            if (CreateTime + DelayTime < Time.time)
            {
                Destroy(gameObject);
            }
        }
    }
}