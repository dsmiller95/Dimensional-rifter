using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;

namespace TradeModeling.Economics
{
    public class CombinedTransactionModel<Resource> where Resource : IComparable
    {
        private Dictionary<(Resource, Resource), (float, float)> transactionMaps;
        private ISet<Resource> mappedResources;
        ///private Dictionary<Resource, int> mappedResourceIndexes;
        public CombinedTransactionModel(IList<Resource> mappedResources)
        {
            this.mappedResources = new HashSet<Resource>(mappedResources);
            var sortedResources = mappedResources.ToList();
            sortedResources.Sort((a, b) => b.CompareTo(a));
            this.transactionMaps = sortedResources
                .SelectMany((resource, index) =>
                    sortedResources
                        .Skip(index + 1)
                        .Select(innerResource => (resource, innerResource))
                    )
                .ToDictionary(x => x, x => (0f, 0f));
        }

        /// <summary>
        /// create a new combined transaction model, by summing the resources from two other models
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        private CombinedTransactionModel(CombinedTransactionModel<Resource> first, CombinedTransactionModel<Resource> second)
        {
            if (!first.mappedResources.SequenceEqual(second.mappedResources))
            {
                throw new ArgumentException("cannot combine transaction models modeling different materials");
            }
            this.mappedResources = new HashSet<Resource>(first.mappedResources);

            this.transactionMaps = first.transactionMaps.ToDictionary(
                kvp => kvp.Key,
                kvp => {
                    var firstTransaction = kvp.Value;
                    var secondTransaction = second.transactionMaps[kvp.Key];
                    var transactionSum = (
                        firstTransaction.Item1 + secondTransaction.Item1,
                        firstTransaction.Item2 + secondTransaction.Item2);
                    return transactionSum;
                });
        }

        /// <summary>
        /// create a new combined transaction model, by cloning a different transaction
        /// </summary>
        /// <param name="toClone">The transaction to clone</param>
        private CombinedTransactionModel(CombinedTransactionModel<Resource> toClone)
        {
            this.mappedResources = new HashSet<Resource>(toClone.mappedResources);

            this.transactionMaps = toClone.transactionMaps.ToDictionary(
                kvp => kvp.Key,
                kvp =>  (kvp.Value.Item1, kvp.Value.Item2)
                );
        }

        public (float, float) GetTransactionAmounts(Resource sold, Resource bought)
        {
            if (sold.CompareTo(bought) > 0)
            {
                return transactionMaps[(sold, bought)];
            } else if (sold.CompareTo(bought) < 0)
            {
                var transaction = transactionMaps[(bought, sold)];
                return (transaction.Item2, transaction.Item1);
            }
            throw new ArgumentException("both resources cannot be equal");
        }

        public void SetTransactionValue(Resource sold, Resource bought, float soldAmount, float boughtAmount)
        {
            if (sold.CompareTo(bought) > 0)
            {
                transactionMaps[(sold, bought)] = (soldAmount, boughtAmount);
            }
            else if (sold.CompareTo(bought) < 0)
            {
                transactionMaps[(bought, sold)] = (boughtAmount, soldAmount);
            } else
            {
                throw new ArgumentException("both resources cannot be equal");
            }
        }
        public void AddTransaction(Resource sold, Resource bought, float soldAmount, float boughtAmount)
        {
            if (sold.CompareTo(bought) > 0)
            {
                var transaction = transactionMaps[(sold, bought)];
                transactionMaps[(sold, bought)] = (transaction.Item1 + soldAmount, transaction.Item2 + boughtAmount);
            } else if (sold.CompareTo(bought) < 0)
            {
                var reverseTransaction = transactionMaps[(bought, sold)];
                transactionMaps[(bought, sold)] = (reverseTransaction.Item1 + boughtAmount, reverseTransaction.Item2 + soldAmount);
            } else
            {
                throw new ArgumentException("both resources cannot be equal");
            }
        }

        public static Dictionary<Resource, float> operator +(
            Dictionary<Resource, float> resourceCount, 
            CombinedTransactionModel<Resource> transaction)
        {
            var newResources = new Dictionary<Resource, float>(resourceCount);
            foreach (var singleTransaction in transaction.transactionMaps)
            {
                newResources[singleTransaction.Key.Item1] += singleTransaction.Value.Item1;
                newResources[singleTransaction.Key.Item2] += singleTransaction.Value.Item2;
            }
            return newResources;
        }

        public static CombinedTransactionModel<Resource> operator -(CombinedTransactionModel<Resource> a)
        {
            var newModel = new CombinedTransactionModel<Resource>(a);
            newModel.transactionMaps = newModel.transactionMaps.ToDictionary(kvp => kvp.Key, kvp => (-kvp.Value.Item1, -kvp.Value.Item2));
            return newModel;
        }

        public static CombinedTransactionModel<Resource> operator +(CombinedTransactionModel<Resource> a, CombinedTransactionModel<Resource> b)
        {
            return new CombinedTransactionModel<Resource>(a, b);
        }
        public static Dictionary<Resource, float> operator -(
            Dictionary<Resource, float> resourceCount,
            CombinedTransactionModel<Resource> transaction)
        {
            return resourceCount + (-transaction);
        }
        public static Dictionary<Resource, float> operator +(
            CombinedTransactionModel<Resource> transaction,
            Dictionary<Resource, float> resourceCount)
        {
            return resourceCount + transaction;
        }
    }
}
