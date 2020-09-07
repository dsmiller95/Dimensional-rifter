using Assets.Behaviors.Scripts.Utility_states;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using System;
using System.Linq;
using TradeModeling.Inventories;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Supplying : ContinueableState<TileMapMember>
    {
        private Func<TileMapMember, bool> supplyTarget;
        private GenericSelector<IInventory<Resource>> inventoryToSupplyFrom;

        public Supplying(GenericSelector<IInventory<Resource>> inventoryToSupplyFrom, Func<TileMapMember, bool> supplyTargetFilter)
        {
            supplyTarget = supplyTargetFilter;
            this.inventoryToSupplyFrom = inventoryToSupplyFrom;
        }

        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var seekResult = tileMember.SeekClosestOfType(supplyTarget);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var suppliables = seekResult.reached.GetComponents<Suppliable>().Where(x => x.CanRecieveSupply());
                var stateSource = data.GetComponent<VariableInstantiator>();
                var selfInv = inventoryToSupplyFrom.GetCurrentValue(stateSource);

                foreach (var supply in suppliables)
                {
                    supply.SupplyInto(selfInv);
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
