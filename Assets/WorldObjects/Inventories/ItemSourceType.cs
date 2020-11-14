﻿using Assets.WorldObjects.Members;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "ItemSourceType", menuName = "Members/Inventory/ItemSourceType", order = 1)]
    public class ItemSourceType : IDableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif

        public int ID;

        public override void AssignId(int myNewID)
        {
            ID = myNewID;
        }
    }
}
