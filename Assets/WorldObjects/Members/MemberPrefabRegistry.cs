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

    [CreateAssetMenu(fileName = "TileMemberTypePrefabRegistry", menuName = "Saving/MembersPrefabRegistry", order = 1)]
    public class MemberPrefabRegistry : ScriptableObject
    {
        public PrefabRegistration[] prefabs;

        private IDictionary<MemberType, TileMapMember> prefabDictionary;

        private void OneTimeSetupPrefabDictionary()
        {
            Debug.Log("Prefab registry awake");
            prefabDictionary = prefabs.ToDictionary(x => x.type, x => x.memberPrefab);

            if (Enum.GetValues(typeof(MemberType)).Cast<MemberType>().Any(type => !prefabDictionary.ContainsKey(type)))
            {
                throw new Exception("No all member types are present in the prefab registration");
            }
        }

        public TileMapMember GetPrefabForType(MemberType type, Transform parent)
        {
            if(prefabDictionary == null)
            {
                this.OneTimeSetupPrefabDictionary();
            }
            var prefab = prefabDictionary[type];
            return Instantiate(prefab, parent).GetComponent<TileMapMember>();
        }
    }
}
