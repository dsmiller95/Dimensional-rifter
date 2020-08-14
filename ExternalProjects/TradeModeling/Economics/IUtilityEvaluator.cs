using TradeModeling.Inventories.Adapter;

namespace TradeModeling.Economics
{
    /// <summary>
    /// An object which can give the current incremental utility; keeping track of an inventory
    /// </summary>
    public interface IUtilityEvaluator<Resource, Self> where Self : IExchangeInventory
    {
        /// <summary>
        /// Calculate the utility of getting increment more
        ///     Acts like an integral; if increment is negative utility will be inverted
        /// </summary>
        /// <param name="inventory">The inventory to operate on</param>
        /// <param name="increment">the additional amount of the item</param>
        /// <returns>The additional utility from gaining more item</returns>
        float GetIncrementalUtility(Resource type, Self inventory, float increment);

        /// <summary>
        /// Calculate the total utility of <paramref name="type"/> in <paramref name="inventory"/>
        /// </summary>
        /// <param name="type">The item type</param>
        /// <param name="inventory">The inventory to operate on</param>
        /// <returns>The additional utility from gaining more item</returns>
        float GetTotalUtility(Resource type, Self inventory);
    }
}
