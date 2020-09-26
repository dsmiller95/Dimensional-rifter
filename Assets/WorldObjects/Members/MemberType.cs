using Assets.WorldObjects.SaveObjects;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members
{

    [Serializable]
    [CreateAssetMenu(fileName = "MemberType", menuName = "Members/MemberType/Type", order = 2)]
    public class MemberType : IDableObject
    {
        [SerializeField]
        public int memberID;

        [Tooltip("When true this member will be included in the list of possible recepticals for a storage task")]
        public bool recieveStorage = false;

        public GameObject memberPrefab;

        public virtual InMemberObjectData[] InstantiateNewSaveObject()
        {
            return new InMemberObjectData[0];
        }

        public override bool Equals(object obj)
        {
            if (obj is MemberType other)
            {
                return other.memberID == memberID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return memberID;
        }

        public override void AssignId(int myNewID)
        {
            memberID = myNewID;
        }
    }
}
