using UnityEngine;

namespace Assets.WorldObjects.Members.Building
{
    [RequireComponent(typeof(TileMapMember))]
    public class SelfTileMemberReplacer : MonoBehaviour
    {
        public TileMapMember memberPrefab;

        public static void ReplaceMember(GameObject self, TileMapMember newPrefab)
        {
            var myMember = self.GetComponent<TileMapMember>();

            var newMember = Instantiate(newPrefab, self.transform.parent).GetComponent<TileMapMember>();
            newMember.SetPosition(myMember);
            Destroy(self);
        }

        public void InstantiateMemberAtCurrentLocation()
        {
            ReplaceMember(gameObject, memberPrefab);
        }
    }
}
