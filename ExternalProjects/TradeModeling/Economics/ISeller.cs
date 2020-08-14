using TradeModeling.Inventories;
using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Economics
{
    public interface ISeller<Resource, Self, Other>
        where Self : class, IExchangeInventory
        where Other : class, IExchangeInventory
    {
        /// <summary>
        /// Sell of a certain amount of the resource. Should -only- change the state of <paramref name="inventory"/>
        ///     selfInventory is always the inventory to be sold into -- this is a sell from the perspective of the self
        /// </summary>
        /// <param name="amount">the amount to sell</param>
        /// <param name="execute">Whether or not to actually execute the sell</param>
        /// <param name="selfInventory">The inventory to sell from</param>
        /// <param name="otherInventory">The inventory to sell to</param>
        /// <returns>the amount gained from selling exactly amount</returns>
        ActionOption<ExchangeResult<Resource>> Sell(Resource type, float amount, Self selfInventory, Other otherInventory);

        /// <summary>
        /// Determines whether or not this seller is capable of executing a sell at this time.
        ///     An example of when it cannot execute a sell is when the inventory is empty
        /// </summary>
        /// <param name="selfInventory">The inventory selling from</param>
        /// <param name="otherInventory">The inventory to sell to</param>
        /// <returns>whether or not a sell of any amount can be executed</returns>
        bool CanSell(Resource type, Self selfInventory, Other otherInventory);
    }
}
