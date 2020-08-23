using UnityEngine;

namespace Assets.WorldObjects.Members
{
    [RequireComponent(typeof(TileMapMember))]
    public class Buildable : MonoBehaviour
    {
        //TODO: convert this to take a scriptable object identity of the member?
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
