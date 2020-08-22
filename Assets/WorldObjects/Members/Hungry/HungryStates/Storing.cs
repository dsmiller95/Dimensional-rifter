﻿using Assets.Behaviors;
using Assets.Behaviors.UtilityStates;
using TradeModeling.Inventories;

namespace Assets.WorldObjects.Members.Hungry.HungryStates
{
    public class Storing : GenericStateHandler<Hungry>
    {
        public GenericStateHandler<Hungry> HandleState(Hungry data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            if (tileMember.SeekClosestOfType(member => member.gameObject.GetComponent<Storage>() != null) == NavigationAttemptResult.ARRIVED)
            {
                var storage = tileMember.currentTarget.GetComponent<Storage>();
                var storageInv = storage.GetComponent<ResourceInventory>();
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