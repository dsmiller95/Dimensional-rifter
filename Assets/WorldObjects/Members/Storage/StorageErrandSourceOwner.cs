using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    [System.Obsolete("Use Entities")]
    public class StorageErrandSourceOwner : MonoBehaviour
    {
        public StorageErrandSource source;

        private void Awake()
        {
            source.Init();
        }
    }
}
