using UnityEngine;

namespace Assets.WorldObjects.Members.Food
{
    [RequireComponent(typeof(TileMapMember))]
    public class Food : MonoBehaviour, IMemberSaveable
    {
        public MemberType GetMemberType()
        {
            return MemberType.FOOD;
        }

        public static object GenerateNewSaveObject()
        {
            return null;
        }

        public object GetSaveObject()
        {
            return null;
        }

        public void SetupFromSaveObject(object save)
        {
        }
    }
}
