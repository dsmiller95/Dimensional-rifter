using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    public class StorageErrandSourceOwner : MonoBehaviour
    {
        public StorageErrandSource source;

        private void Awake()
        {
            source.Init();
        }
    }
}
