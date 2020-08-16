using Assets.Behaviors;
using Assets.Behaviors.UtilityStates;
using Assets.WorldObjects;
using TradeModeling.Inventories;

namespace Assets.WorldObjects.Members.Hungry.HungryStates
{
    public class Eating : GenericStateHandler<Hungry>
    {
        public Eating()
        {
        }
        public GenericStateHandler<Hungry> HandleState(Hungry data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            if (tileMember.SeekClosestOfType(member =>
                {
                    var storage = member.gameObject.GetComponent<Storage>();
                    if (storage == null)
                    {
                        return false;
                    }
                    return storage.GetComponent<ResourceInventory>().inventory.Get(Resource.FOOD) > 0;
                }))
            {
                var storage = tileMember.currentTarget.GetComponent<Storage>();
                var storageInv = storage.GetComponent<ResourceInventory>();

                var consume = storageInv.inventory.Consume(Resource.FOOD, data.currentHunger);
                data.currentHunger -= consume.info;
                consume.Execute();

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