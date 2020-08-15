using Assets.WorldObjects.Members;
using UnityEngine;

namespace Assets.WorldObjects
{
    [RequireComponent(typeof(TileMapMember))]
    public class Food : MonoBehaviour, IMemberSaveable
    {
        public MemberType GetMemberType()
        {
            return MemberType.FOOD;
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
