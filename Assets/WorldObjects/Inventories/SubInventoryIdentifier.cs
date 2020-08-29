using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Inventories
{

    [CreateAssetMenu(fileName = "SubInventory", menuName = "Members/Inventory/UnlimitedInventory", order = 1)]
    public class SubInventoryIdentifier : ScriptableObject
    {
        public string SubInventoryId;
        public SaveableInventoryAmount[] DefaultAmount;

        public IInventory<Resource> GenerateInventoryWithDefaultAmounts()
        {
            var initialInventory = new Dictionary<Resource, float>();
            var resourceTypes = Enum.GetValues(typeof(Resource)).Cast<Resource>();
            foreach (var resource in resourceTypes)
            {
                // create the key with default. Emit set events in Start(); once everyone has had a chance to subscribe to updates
                initialInventory[resource] = 0;
            }

            var inventory = new BasicInventory<Resource>(
                initialInventory);
            
            foreach (var startingAmount in DefaultAmount)
            {
                inventory.SetAmount(startingAmount.type, startingAmount.amount).Execute();
            }

            return inventory;
        }
    }
}
