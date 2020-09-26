using UnityEngine;

namespace Assets.WorldObjects.Members
{
    [CreateAssetMenu(fileName = "TileMemberTypePrefabRegistry", menuName = "Members/MemberType/MembersPrefabRegistry", order = 1)]
    public class MembersRegistry : UniqueObjectRegistryWithAccess<MemberType>
    {
    }
}
