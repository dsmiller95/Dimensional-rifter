using TradeModeling.Economics;
using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Exchanges
{
    public interface IExchangeAdapter<T> : IExchange<T, TradingInventoryAdapter<T>, TradingInventoryAdapter<T>>
    {
    }
}
