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

            var currentRegion = myMember.currentRegion;

            var newMember = Instantiate(newPrefab, currentRegion.transform).GetComponent<TileMapMember>();
            newMember.SetPosition(myMember.CoordinatePosition, currentRegion);
            Destroy(self);
        }

        public void InstantiateMemberAtCurrentLocation()
        {
            ReplaceMember(gameObject, memberPrefab);
        }
    }
}
