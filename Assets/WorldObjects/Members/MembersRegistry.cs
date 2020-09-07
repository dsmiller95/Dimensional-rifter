using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.WorldObjects.Members
{
    [Serializable]
    public struct PrefabRegistration
    {
        public MemberType type;
        public TileMapMember memberPrefab;
    }

    [CreateAssetMenu(fileName = "TileMemberTypePrefabRegistry", menuName = "Members/MemberType/MembersPrefabRegistry", order = 1)]
    public class MembersRegistry : ScriptableObject
    {
        public MemberType[] allTypes;

        private IDictionary<MemberTypeUniqueData, MemberType> memberTypeDictionary;

        private void OneTimeSetupPrefabDictionary()
        {
            Debug.Log("Prefab registry awake");
            memberTypeDictionary = allTypes.ToDictionary(x => x.uniqueData, x => x);
        }

        public MemberType GetMemberFromUniqueInfo(MemberTypeUniqueData type)
        {
            if (memberTypeDictionary == null)
            {
                OneTimeSetupPrefabDictionary();
            }
            var memberType = memberTypeDictionary[type];
            if (!memberType || memberType == null)
            {
                return null;
            }
            return memberType;
        }
    }
}
