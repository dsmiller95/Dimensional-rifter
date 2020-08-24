using UnityEngine;

namespace Assets.WorldObjects.Members.Building
{
    [RequireComponent(typeof(TileMapMember))]
    public class SelfTileMemberReplacer : MonoBehaviour
    {
        public TileMapMember memberPrefab;

        public void InstantiateMemberAtCurrentLocation()
        {
            var myMember = GetComponent<TileMapMember>();

            var currentRegion = myMember.currentRegion;

            var newMember = Instantiate(memberPrefab, currentRegion.transform).GetComponent<TileMapMember>();
            newMember.SetPosition(myMember.CoordinatePosition, currentRegion);
        }

        public void DestroyBuildable()
        {
            Destroy(this.gameObject);
        }
    }
}
