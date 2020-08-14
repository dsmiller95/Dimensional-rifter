using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Economics
{
    public interface IExchange<T, Self, Other> :
        ISeller<T, Self, Other>,
        IPurchaser<T, Self, Other>
        where Self : class, IExchangeInventory
        where Other : class, IExchangeInventory
    {
    }
}
