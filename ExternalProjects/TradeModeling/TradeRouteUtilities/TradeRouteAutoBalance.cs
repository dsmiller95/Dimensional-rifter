using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;

namespace TradeModeling.TradeRouteUtilities
{
    [Serializable]
    public class ResourceTrade<T> where T : System.Enum
    {
        /// <summary>
        /// The type of resource to move
        /// </summary>
        public T type;
        /// <summary>
        /// the amount of resource to move into the market's inventory. Set to a negative value to withdraw
        ///     resources from the market
        /// </summary>
        public float amount;
    }

    public static class TradeRouteAutoBalance
    {
        public static ResourceTrade<T>[][] GetTradesWhichBalanceInventories<T>(
            IInventory<T> inventoryToDistribute,
            IList<IInventory<T>> inventories,
            IList<Dictionary<T, float>> maximumAmounts,
            T[] resourcesToTrade,
            bool roundToInts = false)
            where T : System.Enum
        {
            var totalSurplusToDistribute = resourcesToTrade.ToDictionary(x => x, x => inventoryToDistribute.Get(x));
            SumInto(totalSurplusToDistribute, inventories);

            //start at a target of 0. build up based on the total surplus we can distribute
            var actualTargetInventories = new List<IDictionary<T, float>>(inventories.Count);
            for(var i = 0; i < inventories.Count; i++)
            {
                actualTargetInventories.Add(resourcesToTrade.ToDictionary(x => x, x => 0f));
            }

            foreach (var resource in resourcesToTrade)
            {
                var eligibleInventoryIndexes = actualTargetInventories.Select((inv, index) => index).ToList();

                // Increment the target amount for each inventory by a constant amount
                //  until the surplus for this resource is exhausted, or until no inventories can
                //  take more surplus
                // TODO: see if this logic can be extracted into something less domain-specific. it's a lot of numbers and inventory access
                //  would be nice to convert to a function that just takes in the numbers for one specific resource and spits out the target amounts
                while (eligibleInventoryIndexes.Count > 0 && totalSurplusToDistribute[resource] > 1e-5)
                {
                    var nextEligibleInventoryIndexes = new List<int>();
                    var availableIncrementPerInventory = totalSurplusToDistribute[resource] / eligibleInventoryIndexes.Count;
                    var totalTakenFromSurplus = 0f;
                    foreach (var inventoryIndex in eligibleInventoryIndexes)
                    {
                        var inventory = actualTargetInventories[inventoryIndex];
                        var currentAmount = inventory[resource];
                        var maximumAmount = maximumAmounts[inventoryIndex][resource];

                        var maximumAppliableIncrement = Math.Min(availableIncrementPerInventory, maximumAmount - currentAmount);
                        if (maximumAppliableIncrement >= availableIncrementPerInventory - 1e-5)
                        {
                            // this inventory fit all of the stuff given. keep it around for the next round of resource distribution 
                            nextEligibleInventoryIndexes.Add(inventoryIndex);
                        }
                        inventory[resource] += maximumAppliableIncrement;
                        totalTakenFromSurplus += maximumAppliableIncrement;
                    }
                    totalSurplusToDistribute[resource] -= totalTakenFromSurplus;
                    eligibleInventoryIndexes = nextEligibleInventoryIndexes;
                }
            }

            var tradeAmounts = inventories
                .Zip(actualTargetInventories, (inventory, target) => new { inventory, target })
                .Select(inventory =>
                    GetResourceTradesFromInventoryToTargetAmount(inventory.inventory, inventory.target, roundToInts)
                .Where(trade => trade.amount != 0)
                .ToArray()
                );

            return tradeAmounts.ToArray();
        }

        private static void SumInto<T>(Dictionary<T, float> seed, IList<IInventory<T>> inventories)
            where T : Enum
        {
            foreach (var inventory in inventories)
            {
                foreach (var resource in seed.Keys.ToList())
                {
                    seed[resource] += inventory.Get(resource);
                }
            }
        }

        private static IEnumerable<ResourceTrade<T>> GetResourceTradesFromInventoryToTargetAmount<T>(IInventory<T> inventory, IDictionary<T, float> targetInventoryAmount, bool roundToInt)
            where T : System.Enum
        {
            foreach (var resource in targetInventoryAmount.Keys)
            {
                var resourceDiff = targetInventoryAmount[resource] - inventory.Get(resource);
                if (roundToInt)
                {
                    //truncate to int. this has the effect of always rounding "down"
                    //  AKA reducing the distance from 0
                    resourceDiff = (int)resourceDiff;
                }
                yield return new ResourceTrade<T>
                {
                    type = resource,
                    amount = resourceDiff
                };
            }
        }
    }
}
