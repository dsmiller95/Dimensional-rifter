﻿using Assets.Behaviors;
using Assets.Behaviors.UtilityStates;
using Assets.WorldObjects;
using TradeModeling.Inventories;

namespace Assets.WorldObjects.Members.Hungry.HungryStates
{
    public class Eating : IGenericStateHandler<Hungry>
    {
        public Eating()
        {
        }
        public IGenericStateHandler<Hungry> HandleState(Hungry data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var seekResult = tileMember.SeekClosestOfType(member =>
            {
                if (!member.memberType.recieveStorage)
                {
                    return false;
                }
                var storage = member.gameObject.GetComponent<ResourceInventory>();
                return storage.inventory.Get(Resource.FOOD) > 0;
            });
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var storageInv = seekResult.reached.GetComponent<ResourceInventory>();

                var consume = storageInv.inventory.Consume(Resource.FOOD, data.currentHunger);
                data.currentHunger -= consume.info;
                consume.Execute();

                var waiter = new Waiting<Hungry>();
                waiter.Finalize(.5f, new HungryDecider());
                return waiter;
            }
            if(seekResult.status == NavigationStatus.INVALID_TARGET)
            {
                return new HungryDecider();
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