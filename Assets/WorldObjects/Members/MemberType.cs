using Assets.WorldObjects.SaveObjects;
using Dman.ObjectSets;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members
{

    [Serializable]
    [CreateAssetMenu(fileName = "MemberType", menuName = "Members/MemberType/Type", order = 2)]
    public class MemberType : IDableObject
    {
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
                return other.myId == this.myId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return myId;
        }
    }
}
