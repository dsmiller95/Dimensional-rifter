using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    [RequireComponent(typeof(TileMapMember))]
    public class Food : MonoBehaviour, IGatherable
    {
        public Resource resourceType = Resource.FOOD;
        public bool CanGather()
        {
            return true;
        }

        public void OnGathered()
        {
            Destroy(gameObject);
        }
        public Resource GatherableType => resourceType;
    }
}
