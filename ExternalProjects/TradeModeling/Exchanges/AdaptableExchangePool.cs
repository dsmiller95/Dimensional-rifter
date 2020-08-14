using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;

namespace TradeModeling.Exchanges
{
    public class AdaptableExchangePool<T> : IEnumerable<SingleExchangeModel<T>>
    {
        private IList<AdaptableExchangeModel<T>> exchangePool;

        public AdaptableExchangePool(int poolSize, AdaptableExchangeModel<T> poolSeed)
        {
            exchangePool = new List<AdaptableExchangeModel<T>>();
            for (; poolSize > 0; poolSize--)
            {
                exchangePool.Add(poolSeed.GetVariant());
            }
        }
        public AdaptableExchangePool(IEnumerable<AdaptableExchangeModel<T>> pool)
        {
            exchangePool = pool.ToList();
        }

        public IEnumerator<SingleExchangeModel<T>> GetEnumerator()
        {
            return exchangePool.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return exchangePool.GetEnumerator();
        }

        public SingleExchangeModel<T> this[int index]
        {
            get => exchangePool[index];
        }
        public int Count => exchangePool.Count;
    }
}
