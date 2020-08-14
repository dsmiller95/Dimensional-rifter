using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Economics
{

    /// <summary>
    /// This class can be used to evaluate which initial resources are responsible for the ending utility
    ///     For Example, if after selling 2 cactus for 2 corn, leaving a total of 3 corn and 0 cactus. When
    ///     the utility of 3 corn is 6, then this algorithm will backtrack across the transaction and distribute
    ///     2/3 of the ending utility. So the returned value would be (6 * 2/3) = 4 utility for cactus,
    ///     and (6 * 1/3) = 2 utility for corn. 
    /// </summary>
    /// <typeparam name="Resource"></typeparam>
    public class UtilityAnalyzer<Resource> where Resource : IComparable
    {
        public Dictionary<Resource, float> GetTotalUtilityByInitialResource(
            IList<Resource> tradeableResources,
            TradingInventoryAdapter<Resource> endingInventory,
            IList<(ExchangeResult<Resource>?, PurchaseOperationResult<Resource>)> transactionLedger,
            IUtilityEvaluator<Resource, TradingInventoryAdapter<Resource>> utilityEvaluator
            )
        {
            var endingUtilities = tradeableResources.ToDictionary(
                resource => resource,
                resource => utilityEvaluator.GetTotalUtility(resource, endingInventory)
              );
            var retractingInventory = tradeableResources.ToDictionary(
                resource => resource,
                resource => endingInventory.Get(resource)
              );

            var transactionListReverse = transactionLedger
                .Select(transaction =>
                    GetTransactionMapFromSingleTransaction(tradeableResources, transaction)
                ).Reverse();

            foreach (var transaction in transactionListReverse)
            {
                DistributeUtilityBasedOnTransaction(tradeableResources, endingUtilities, transaction, retractingInventory);
                retractingInventory = retractingInventory - transaction;
            }

            return endingUtilities;
        }

        /// <summary>
        /// Move the utility through the utilities dictionary based on the amount of items transferred thorugh
        ///     The utility per item is evaluated based on the ending inventory: so any amount that is moved as a result
        ///     of the transaction moves an amount of utility based on the ratio between the amount of resource moved vs the total inventory
        /// </summary>
        /// <param name="utilities"></param>
        /// <param name="transaction"></param>
        private void DistributeUtilityBasedOnTransaction(
            IList<Resource> tradeableResources,
            Dictionary<Resource, float> utilities,
            CombinedTransactionModel<Resource> transaction,
            Dictionary<Resource, float> endingInventory)
        {
            foreach (var sold in tradeableResources)
            {
                foreach (var bought in tradeableResources.Where(x => !EqualityComparer<Resource>.Default.Equals(x, sold)))
                {
                    var amountOfBought = endingInventory[bought];
                    if (amountOfBought == 0)
                    {
                        continue;
                    }
                    var singleTransaction = transaction.GetTransactionAmounts(sold, bought);
                    if (singleTransaction.Item1 < 0)
                    {
                        var soldRatio = singleTransaction.Item2 / amountOfBought;
                        var transferredUtility = utilities[bought] * soldRatio;
                        utilities[bought] -= transferredUtility;
                        utilities[sold] += transferredUtility;
                    }
                }
            }
        }

        public CombinedTransactionModel<Resource> GetTransactionMapFromLedger(
            IList<Resource> tradeableResources,
            IList<(ExchangeResult<Resource>?, PurchaseOperationResult<Resource>)> transactionLedger)
        {

            var allTransactions = new CombinedTransactionModel<Resource>(tradeableResources);

            foreach (var transaction in transactionLedger.Where(x => x.Item1.HasValue))
            {
                var sourceResourcePerResultResource = transaction.Item1.Value.amount / transaction.Item2.exchages.Count;
                foreach (var boughtItem in transaction.Item2.exchages)
                {
                    allTransactions.AddTransaction(
                        transaction.Item1.Value.type,
                        boughtItem.type,
                        -sourceResourcePerResultResource,
                        boughtItem.amount);
                }
            }

            return allTransactions;
        }

        private CombinedTransactionModel<Resource> GetTransactionMapFromSingleTransaction(
            IList<Resource> tradeableResources,
            (ExchangeResult<Resource>?, PurchaseOperationResult<Resource>) transaction)
        {

            var transactionModel = new CombinedTransactionModel<Resource>(tradeableResources);

            if (transaction.Item1.HasValue)
            {
                var sourceResourcePerResultResource = transaction.Item1.Value.amount / transaction.Item2.exchages.Count;
                foreach (var boughtItem in transaction.Item2.exchages)
                {
                    transactionModel.AddTransaction(
                        transaction.Item1.Value.type,
                        boughtItem.type,
                        -sourceResourcePerResultResource,
                        boughtItem.amount);
                }
            }

            return transactionModel;
        }
    }
}
