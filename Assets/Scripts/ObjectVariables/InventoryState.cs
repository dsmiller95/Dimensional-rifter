using Assets.Scripts.Core;
using Assets.Scripts.Utilities;
using Assets.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Scripts.ObjectVariables
{
    [Serializable]
    public class InventorySaveData
    {
        public SaveableInventoryAmount<Resource>[] amounts;
    }


    [CreateAssetMenu(fileName = "InventoryState", menuName = "State/InventoryState", order = 5)]
    public class InventoryState : GenericState<IInventory<Resource>>
    {
        public SaveableInventoryAmount<Resource>[] initialItems;

        public Resource[] validItems;
        public Resource[] spaceFillingItems;
        public int spaceFillingCapacity;

        public override GenericVariable<IInventory<Resource>> GenerateNewVariable()
        {
            var instanced = CreateInstance<InventoryVariable>();
            instanced.SetValue(GenerateInventoryWithDefaultAmounts(
                initialItems,
                validItems,
                spaceFillingItems,
                spaceFillingCapacity));
            return instanced;
        }

        private IInventory<Resource> GenerateInventoryWithDefaultAmounts(
            SaveableInventoryAmount<Resource>[] defaultAmounts,
            Resource[] validResources,
            Resource[] spaceFillingResources,
            int space
            )
        {
            var initialInventory = new Dictionary<Resource, float>();
            foreach (var resource in validResources)
            {
                initialInventory[resource] = 0;
            }

            var inventory = new SpaceFillingInventory<Resource>(
                initialInventory,
                spaceFillingResources,
                space,
                validResources);
            inventory.SetSerializedToInventory(defaultAmounts);
            return inventory;
        }

        public override object GetSaveObjectFromVariable(GenericVariable<IInventory<Resource>> variable)
        {
            var currentInventory = variable.CurrentValue;
            return new InventorySaveData
            {
                amounts = currentInventory.GetSerializableInventoryAmounts()
            };
        }

        public override void SetSaveObjectIntoVariable(GenericVariable<IInventory<Resource>> variable, object savedValue)
        {
            var saveValue = (InventorySaveData)savedValue;
            if (saveValue == null)
            {
                return;
            }
            variable.CurrentValue.SetSerializedToInventory(saveValue.amounts);
        }
    }
}
