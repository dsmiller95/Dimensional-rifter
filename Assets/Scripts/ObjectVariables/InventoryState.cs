using Assets.Scripts.Core;
using Assets.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Scripts.ObjectVariables
{
    [Serializable]
    public struct SaveableInventoryAmount
    {
        public Resource type;
        public float amount;
    }
    [Serializable]
    public class InventorySaveData
    {
        public SaveableInventoryAmount[] amounts;
    }


    [CreateAssetMenu(fileName = "InventoryState", menuName = "State/InventoryState", order = 5)]
    public class InventoryState : GenericState<IInventory<Resource>>
    {
        public SaveableInventoryAmount[] initialItems;

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
            SaveableInventoryAmount[] defaultAmounts,
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
            SetAmountsToInventory(defaultAmounts, inventory);

            return inventory;
        }

        private void SetAmountsToInventory(IEnumerable<SaveableInventoryAmount> amounts, IInventory<Resource> inventory)
        {
            foreach (var startingAmount in amounts)
            {
                inventory.SetAmount(startingAmount.type, startingAmount.amount).Execute();
            }
        }

        public override object GetSaveObjectFromVariable(GenericVariable<IInventory<Resource>> variable)
        {
            var currentInventory = variable.CurrentValue;
            var saveAmounts = currentInventory.GetCurrentResourceAmounts().Select(x => new SaveableInventoryAmount
            {
                type = x.Key,
                amount = x.Value
            }).ToArray();
            return new InventorySaveData
            {
                amounts = saveAmounts
            };
        }

        public override void SetSaveObjectIntoVariable(GenericVariable<IInventory<Resource>> variable, object savedValue)
        {
            var saveValue = (InventorySaveData)savedValue;
            if (saveValue == null)
            {
                return;
            }
            SetAmountsToInventory(saveValue.amounts, variable.CurrentValue);
        }
    }
}
