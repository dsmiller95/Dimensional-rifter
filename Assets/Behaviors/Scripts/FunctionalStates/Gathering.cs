using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using Assets.WorldObjects.Members.Food;
using System;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Gathering : ContinueableState<TileMapMember>
    {

        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var myInv = data.GetComponent<ResourceInventory>();

            var seekResult = tileMember.SeekClosestOfType(GatheringFilter);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var foundFood = seekResult.reached.GetComponent<IGatherable>();

                var drainResult = seekResult.reached.GetComponent<ResourceInventory>()
                    .inventory.DrainAllInto(myInv.inventory, Enum.GetValues(typeof(Resource)) as Resource[]);

                foundFood.OnGathered();
            }
            if (myInv.inventory.Get(Resource.FOOD) > 0)
            {
                return next;
            }
            return this;
        }
        private bool GatheringFilter(TileMapMember member)
        {
            return member.GetComponent<IGatherable>()?.CanGather() ?? false;
        }

        public override void TransitionIntoState(TileMapMember data)
        {
            base.TransitionIntoState(data);
        }
        public override void TransitionOutOfState(TileMapMember data)
        {
        }

        public override string ToString()
        {
            return $"Gathering";
        }
    }
}