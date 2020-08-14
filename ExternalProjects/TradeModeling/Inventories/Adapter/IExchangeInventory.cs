namespace TradeModeling.Inventories.Adapter
{
    public interface IExchangeInventory
    {
        float GetCurrentFunds();

        /// <summary>
        /// Creates a copy of this exchange inventory to be used for evaluating simulated transactions
        ///     Executing transactions on the new inventory shouldn't effect the backing to this inventory
        /// </summary>
        /// <returns>A new simulated clone</returns>
        IExchangeInventory CreateSimulatedClone();
    }
}
