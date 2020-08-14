using System.Collections.Generic;

namespace TradeModeling.Exchanges
{
    public class AdaptableExchangeModel<T> : SingleExchangeModel<T>
    {
        public AdaptableExchangeModel(IDictionary<T, float> sellPrices, IDictionary<T, float> buyPrices, T moneyType) : base(sellPrices, buyPrices, moneyType)
        {
        }
        protected AdaptableExchangeModel(AdaptableExchangeModel<T> other) : base(other)
        {
        }
        public AdaptableExchangeModel(SingleExchangeModel<T> other) : base(other)
        {
        }

        public AdaptableExchangeModel<T> GetVariant()
        {
            return new AdaptableExchangeModel<T>(this);
        }
    }
}