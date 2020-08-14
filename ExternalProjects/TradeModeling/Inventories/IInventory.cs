using System;
using System.Collections.Generic;
using TradeModeling.Economics;

namespace TradeModeling.Inventories
{
    /// <summary>
    /// Represents the most basic form of an inventory: it only holds items
    ///     and does not interact with other inventories directly.
    /// Used to build inventories that are backed by multiple other inventories,
    ///     or any other bit of data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInventory<T>
    {
        Action<T, float> OnResourceChanged { set; }

        Dictionary<T, float> GetCurrentResourceAmounts();

        float Get(T type);

        IEnumerable<T> GetAllResourceTypes();

        /// <summary>
        /// Sets the amount of a given type in this inventory
        /// </summary>
        /// <param name="type">the type of resource</param>
        /// <param name="amount">the new amount of the resour</param>
        /// <returns>An option to execute the transfer, wrapping the actual amount that will be set in the inventory</returns>
        ActionOption<float> SetAmount(T type, float amount);

        /// <summary>
        /// Determine if it's possible to fit any more of the given item in this inventory
        /// </summary>
        /// <param name="resource">The item to attempt to fit</param>
        /// <returns>True if its possible to fit any amount of <paramref name="resource"/> into this inventory</returns>
        bool CanFitMoreOf(T resource);

        /// <summary>
        /// Create an item source which will simulate the interactions on this inventory
        ///     this should include things like space restrictions and the current amount of items in the inventory
        ///     Should not copy event bindings or external references
        /// </summary>
        /// <returns>A simulated clone</returns>
        IInventory<T> CloneSimulated();
    }

    public static class ItemSourceExtensions
    {

        /// <summary>
        /// Attempts to add as much of amount as possible into this inventory.
        /// </summary>
        /// <param name="type">the type of resource to add</param>
        /// <param name="amount">the maximum amount of resource to add to the inventory</param>
        /// <returns>An option to execute the transfer, wrapping the amount of the resource that was actually added</returns>
        public static ActionOption<float> Add<T>(this IInventory<T> inventory, T type, float amount)
        {
            if (amount < 0)
            {
                throw new NotImplementedException("cannot add a negative amount. use Consume for that purpose");
            }

            var currentAmount = inventory.Get(type);
            return inventory.SetAmount(type, currentAmount + amount)
                .Then(newAmount => newAmount - currentAmount);
        }

        /// <summary>
        /// Consume up to a certain amount out of the inventory
        /// </summary>
        /// <param name="type">the type to consume</param>
        /// <param name="amount">the amount to attempt to consume</param>
        /// <returns>an optional action representing the amount that was actually consumed</returns>
        public static ActionOption<float> Consume<T>(this IInventory<T> inventory, T type, float amount)
        {
            if (amount < 0)
            {
                throw new NotImplementedException();
            }

            var currentAmount = inventory.Get(type);
            return inventory.SetAmount(type, currentAmount - amount)
                .Then(newAmount => currentAmount - newAmount);

        }

        /// <summary>
        /// Drain all the items from this inventory of type res
        /// </summary>
        /// <param name="target">the inventory to drain the items into</param>
        /// <param name="res">the type of items to transfer. Could be a flags</param>
        /// <returns>A map from the resource type to the amount transfered</returns>
        public static Dictionary<T, float> DrainAllInto<T>(this IInventory<T> inventory, IInventory<T> target, T[] itemTypes)
        {
            var result = new Dictionary<T, float>();
            foreach (var itemType in itemTypes)
            {
                var transferOption = inventory.TransferResourceInto(itemType, target, inventory.Get(itemType));

                result[itemType] = transferOption.info;
                transferOption.Execute();
            }
            return result;
        }

        /// <summary>
        /// Transfer <paramref name="amount"/> of <paramref name="type"/> into <paramref name="target"/>
        /// </summary>
        /// <param name="type">the type of resource to transfer</param>
        /// <param name="target">the inventory to transfer into</param>
        /// <param name="amount">the amount to transfer</param>
        /// <returns>An option to execute the transfer, wrapping the amount which would be transferred</returns>
        public static ActionOption<float> TransferResourceInto<T>(this IInventory<T> inventory, T type, IInventory<T> target, float amount)
        {
            if (amount < 0)
            {
                return target.TransferResourceInto(type, inventory, -amount)
                    .Then(added => -added);
            }
            var currentAmount = inventory.Get(type);

            var possibleNewAmount = inventory.SetAmount(type, currentAmount - amount).info;
            var idealAddAmount = currentAmount - possibleNewAmount;

            return target
                .Add(type, idealAddAmount)
                .Then(addedAmount => inventory.SetAmount(type, currentAmount - addedAmount))
                .Then(newAmount => currentAmount - newAmount);
        }

        public static string SerializeDictionaryAmounts<T>(this IInventory<T> inventory, Func<T, string> serializer)
        {
            return MyUtilities.SerializeDictionary(inventory.GetCurrentResourceAmounts(), serializer, num => num.ToString());
        }
    }
}
