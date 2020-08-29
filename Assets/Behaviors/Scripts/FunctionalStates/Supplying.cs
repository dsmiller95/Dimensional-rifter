using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using System;
using System.Linq;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Supplying : ContinueableState<TileMapMember>
    {
        private Func<TileMapMember, bool> supplyTarget;
        public Supplying(Func<TileMapMember, bool> supplyTargetFilter)
        {
            supplyTarget = supplyTargetFilter;
        }

        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var seekResult = tileMember.SeekClosestOfType(supplyTarget);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var suppliables = seekResult.reached.GetComponents<Suppliable>().Where(x => x.CanRecieveSupply());
                var sourceInv = data.GetComponent<ResourceInventory>();

                foreach (var supply in suppliables)
                {
                    supply.SupplyInto(sourceInv.inventory);
                }

                return next;
            }
            return this;
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
            return $"Supplying";
        }
    }
}
