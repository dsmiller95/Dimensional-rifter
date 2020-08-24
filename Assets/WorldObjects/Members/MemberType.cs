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
    [CreateAssetMenu(fileName = "MemberType", menuName = "Saving/MemberType", order = 1)]
    public class MemberType : ScriptableObject
    {
        [SerializeField]
        public MemberTypeUniqueData uniqueData;

        [Tooltip("When true this member will be included in the list of possible recepticals for a storage task")]
        public bool recieveStorage = false;

        public GameObject memberPrefab;

        public object InstantiateNewSaveObject()
        {
            return null;
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
