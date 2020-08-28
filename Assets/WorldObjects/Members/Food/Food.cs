using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    [RequireComponent(typeof(TileMapMember))]
    public class Food : MonoBehaviour, IGatherable
    {
        public bool CanGather()
        {
            return true;
        }

        public void OnGathered()
        {
            Destroy(gameObject);
        }
    }
}
