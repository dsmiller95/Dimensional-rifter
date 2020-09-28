using System;
using System.Linq;

namespace TradeModeling.Inventories
{
    [Serializable]
    public struct SaveableInventoryAmount<T> where T : Enum
    {
        public T type;
        public float amount;
    }
    public static class InventorySerializerExtensions
    {
        public static void SetSerializedToInventory<T>(
            this IInventory<T> inventory,
            SaveableInventoryAmount<T>[] amounts) where T : Enum
        {
            inventory.ConsumeAll();
            foreach (var startingAmount in amounts)
            {
                inventory.SetAmount(startingAmount.type, startingAmount.amount).Execute();
            }
        }

        public static SaveableInventoryAmount<T>[] GetSerializableInventoryAmounts<T>(
            this IInventory<T> inventory) where T : Enum
        {
            return inventory.GetCurrentResourceAmounts().Select(x => new SaveableInventoryAmount<T>
            {
                type = x.Key,
                amount = x.Value
            }).ToArray();
        }
    }
}
