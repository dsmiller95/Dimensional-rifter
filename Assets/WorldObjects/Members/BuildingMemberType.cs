using System;
using UnityEngine;

namespace Assets.WorldObjects.Members
{
    [Serializable]
    public struct ResourceRequirement
    {
        public float amount;
        public Resource type;
    }
    [CreateAssetMenu(fileName = "BuildingType", menuName = "Saving/BuildingType", order = 1)]
    public class BuildingMemberType : MemberType
    {
        public ResourceRequirement resourceCost;
    }
}
