using Assets.WorldObjects.SaveObjects;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members
{

    [Serializable]
    public struct MemberTypeUniqueData
    {
        public int uniqueId;
        public override bool Equals(object obj)
        {
            if (obj is MemberTypeUniqueData other)
            {
                return other.uniqueId == uniqueId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return uniqueId;
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "MemberType", menuName = "Members/MemberType/Type", order = 2)]
    public class MemberType : ScriptableObject
    {
        [SerializeField]
        public MemberTypeUniqueData uniqueData;

        [Tooltip("When true this member will be included in the list of possible recepticals for a storage task")]
        public bool recieveStorage = false;

        [Tooltip("When true this member will be included in the list of possible things to gather for a gathering task")]
        public bool gatherableResource = false;

        public GameObject memberPrefab;

        public virtual InMemberObjectData[] InstantiateNewSaveObject()
        {
            return new InMemberObjectData[0];
        }

        public override bool Equals(object obj)
        {
            if (obj is MemberType other)
            {
                return other.uniqueData.Equals(uniqueData);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return uniqueData.GetHashCode();
        }
    }
}
