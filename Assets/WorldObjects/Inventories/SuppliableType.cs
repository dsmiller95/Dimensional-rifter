using Assets.WorldObjects.Members;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "SupplyableType", menuName = "Members/Inventory/SupplyableType", order = 1)]
    public class SuppliableType : IDableObject
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
