using Assets.Behaviors;
using Assets.Behaviors.UtilityStates;
using TradeModeling.Inventories;

namespace Assets.WorldObjects.Members.Hungry.HungryStates
{
    public class Storing : IGenericStateHandler<Hungry>
    {
        public IGenericStateHandler<Hungry> HandleState(Hungry data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var seekResult = tileMember.SeekClosestOfType(member => member.memberType.recieveStorage == true);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var storageInv = seekResult.reached.GetComponent<ResourceInventory>();
                var sourceInv = data.GetComponent<ResourceInventory>();

                sourceInv.inventory.DrainAllInto(storageInv.inventory, new Resource[] { Resource.FOOD });

                var waiter = new Waiting<Hungry>();
                waiter.Finalize(.5f, new HungryDecider());
                return waiter;
            }
            return this;
        }

        public void TransitionIntoState(Hungry data)
        {
        }

        public void TransitionOutOfState(Hungry data)
        {
        }
    }
}
