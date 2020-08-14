using System;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Economics;
using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Exchanges
{
    public class SingleExchangeModel<T> : IExchangeAdapter<T>
    {
        protected IDictionary<T, float> marketSellRates;
        protected IDictionary<T, float> marketBuyRates;

        private T moneyType;

        public SingleExchangeModel(IDictionary<T, float> sellPrices, IDictionary<T, float> buyPrices, T moneyType)
        {
            marketBuyRates = buyPrices;
            marketSellRates = sellPrices;
            this.moneyType = moneyType;
        }
        public SingleExchangeModel(IDictionary<T, float> exchangeRates, T moneyType) : this(exchangeRates, exchangeRates, moneyType)
        { }
        public SingleExchangeModel(SingleExchangeModel<T> other) :
            this(
                other.marketSellRates.ToDictionary(x => x.Key, x => x.Value),
                other.marketBuyRates.ToDictionary(x => x.Key, x => x.Value),
                other.moneyType)
        { }

        private ActionOption<ExchangeResult<T>> ExchangeInto(T type, float amount, TradingInventoryAdapter<T> targetInventory, TradingInventoryAdapter<T> sourceInventory, float exchangeRate)
        {
            var maxPurchase = targetInventory.GetCurrentFunds() / exchangeRate;
            var amountToPurchase = Math.Min(amount, maxPurchase);
            return sourceInventory
                .TransferResourceInto(type, targetInventory, amountToPurchase)
                .Then(withdrawn => new ExchangeResult<T>
                {
                    amount = withdrawn,
                    cost = withdrawn * exchangeRate,
                    type = type
                }, exchangeResult =>
                {
                    targetInventory.TransferResourceInto(moneyType, sourceInventory, exchangeResult.cost).Execute();
                });
        }

        private bool CanExchangeInto(T type, TradingInventoryAdapter<T> targetInventory, TradingInventoryAdapter<T> sourceInventory)
        {
            return sourceInventory.Get(type) > 0
                && targetInventory.Get(moneyType) > 0
                && targetInventory.CanFitMoreOf(type);
        }

        public ActionOption<ExchangeResult<T>> Purchase(T type, float amount, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            // using the selling rate because this is a purchase from the "other". I.E. a sell from the perspective of the market
            var exchangeRate = marketSellRates[type];
            return ExchangeInto(type, amount, selfInventory, marketInventory, exchangeRate);
        }

        public bool CanPurchase(T type, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            return CanExchangeInto(type, selfInventory, marketInventory);
        }

        public ActionOption<ExchangeResult<T>> Sell(T type, float amount, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            // using the buying rate because this is a sell from the "other". I.E. a purchase from the market
            var exchangeRate = marketBuyRates[type];
            return ExchangeInto(type, amount, marketInventory, selfInventory, exchangeRate);
        }

        public bool CanSell(T type, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            return CanExchangeInto(type, marketInventory, selfInventory);
        }
    }
}