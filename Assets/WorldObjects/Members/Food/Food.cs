using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    [RequireComponent(typeof(TileMapMember))]
    public class Food : MonoBehaviour, IMemberSaveable
    {
        public object GetSaveObject()
        {
            return null;
        }

        public string IdentifierInsideMember()
        {
            return "Food";
        }

        public void SetupFromSaveObject(object save)
        {
        }
    }
}
