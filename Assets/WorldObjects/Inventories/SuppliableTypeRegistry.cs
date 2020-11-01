﻿using Assets.WorldObjects.Members;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{
    [CreateAssetMenu(fileName = "SuppliableTypeRegistry", menuName = "Behaviors/SuppliableTypeRegistry", order = 1)]
    public class SuppliableTypeRegistry : UniqueObjectRegistryWithAccess<SuppliableType>
    {
    }
}
