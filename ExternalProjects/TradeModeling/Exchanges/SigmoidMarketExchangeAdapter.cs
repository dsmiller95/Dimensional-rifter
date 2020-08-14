using Extensions;
using System;
using System.Collections.Generic;
using TradeModeling.Economics;
using TradeModeling.Functions;
using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Exchanges
{
    public class SigmoidMarketExchangeAdapter<T> : IExchangeAdapter<T>
    {
        private IDictionary<T, SigmoidFunction> marketSellRates;
        private IDictionary<T, SigmoidFunction> marketBuyRates;
        private T moneyType;

        public SigmoidMarketExchangeAdapter(IDictionary<T, SigmoidFunctionConfig> sellPrices, IDictionary<T, SigmoidFunctionConfig> buyPrices, T moneyType)
        {
            marketBuyRates = buyPrices.SelectDictionary(config => new SigmoidFunction(config));
            marketSellRates = sellPrices.SelectDictionary(config => new SigmoidFunction(config));
            this.moneyType = moneyType;
        }
        public SigmoidMarketExchangeAdapter(IDictionary<T, SigmoidFunctionConfig> exchangeRates, T moneyType) : this(exchangeRates, exchangeRates, moneyType)
        {
        }

        public ActionOption<ExchangeResult<T>> Purchase(T type, float amount, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            //throw new NotImplementedException();
            // using the buying rate because this is a purchase from the "other". I.E. a sell from the perspective of the market
            var exchangeRateFunction = marketSellRates[type];
            var amountLeftAfterMarketsSell = exchangeRateFunction.GetPointFromNetExtraValueFromPoint(-selfInventory.GetCurrentFunds(), marketInventory.Get(type));
            var maxPurchase = marketInventory.Get(type) - amountLeftAfterMarketsSell;

            var amountToPurchase = Math.Min(amount, maxPurchase);
            return marketInventory
                .TransferResourceInto(type, selfInventory, amountToPurchase)
                .Then(withdrawn => new ExchangeResult<T>
                {
                    amount = withdrawn,
                    cost = -exchangeRateFunction.GetIncrementalValue(marketInventory.Get(type), -withdrawn),
                    type = type
                }, exchangeResult =>
                {
                    selfInventory.TransferResourceInto(moneyType, marketInventory, exchangeResult.cost).Execute();
                });
        }

        public bool CanPurchase(T type, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            return marketInventory.Get(type) > 0
                && selfInventory.Get(moneyType) > 0
                && selfInventory.CanFitMoreOf(type);
        }

        public ActionOption<ExchangeResult<T>> Sell(T type, float amount, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            //throw new NotImplementedException();
            // using the buying rate because this is a sell from the "other". I.E. a purchase from the market
            var exchangeRateFunction = marketBuyRates[type];
            var amountLeftAfterMarketsBuy = exchangeRateFunction.GetPointFromNetExtraValueFromPoint(marketInventory.GetCurrentFunds(), marketInventory.Get(type));// marketInventory.GetCurrentFunds() / exchangeRate;
            var maxSell = amountLeftAfterMarketsBuy - marketInventory.Get(type);

            var amountToSell = Math.Min(amount, maxSell);
            return selfInventory
                .TransferResourceInto(type, marketInventory, amountToSell)
                .Then(totalDeposited => new ExchangeResult<T>
                {
                    amount = totalDeposited,
                    cost = exchangeRateFunction.GetIncrementalValue(marketInventory.Get(type), totalDeposited),
                    type = type
                }, exchangeResult =>
                {
                    marketInventory.TransferResourceInto(moneyType, selfInventory, exchangeResult.cost).Execute();
                });
        }

        public bool CanSell(T type, TradingInventoryAdapter<T> selfInventory, TradingInventoryAdapter<T> marketInventory)
        {
            return selfInventory.Get(type) > 0
                && marketInventory.Get(moneyType) > 0
                && marketInventory.CanFitMoreOf(type);
        }
    }
}