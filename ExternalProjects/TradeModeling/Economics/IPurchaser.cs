using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Economics
{

    public struct ExchangeResult<Resource>
    {
        /// <summary>
        /// The amount of money which is paid for the items
        /// </summary>
        public float cost;
        /// <summary>
        /// Actual amount of items purchased. this can be lower than requested if the selling inventory is empty
        /// </summary>
        public float amount;
        public Resource type;
    }
    public interface IPurchaser<Resource, Self, Other>
        where Self : class, IExchangeInventory
        where Other : class, IExchangeInventory
    {
        // TODO: Ensure that when the option is executed it checks to be sure that the inventories have not been modified
        /// <summary>
        /// Purchase given amount. Should -only- change the state of <paramref name="inventory"/>
        ///     selfInventory is always the inventory to be purchased into -- this is a purchase from the perspective of the self
        /// </summary>
        /// <param name="amount">the amount to purchase</param>
        /// <param name="execute">Whether or not to execute the purchase</param>
        /// <param name="selfInventory">The inventory to purchase into</param>
        /// <param name="otherInventory">The inventory to purchase from</param>
        /// <returns>A summary of the transaction</returns>
        ActionOption<ExchangeResult<Resource>> Purchase(Resource type, float amount, Self selfInventory, Other otherInventory);

        /// <summary>
        /// Determines whether or not this purchaser is capable of executing a purchase at this time.
        ///     An example of when it cannot execute a purchase is when the selling inventory is empty
        /// </summary>
        /// <param name="selfInventory">The inventory to purchase into</param>
        /// <param name="otherInventory">The inventory to purchase from</param>
        /// <returns>whether or not a purchase of any amount can be executed</returns>
        bool CanPurchase(Resource type, Self selfInventory, Other otherInventory);
    }
}
