using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using Assets.WorldObjects.Members.Food;
using System;
using System.Linq;
using TradeModeling.Inventories;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Gathering : ContinueableState<TileMapMember>
    {
        private Resource? targetElement;

        public Gathering()
        {

        }

        public Gathering(Resource target)
        {
            targetElement = target;
        }

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
                return next;
            }
            // TODO:
            //  This is to "recover" from a save state where I have something in my inventory
            //  this really should be done at decision-time, not on every loop
            if (myInv.inventory.GetCurrentResourceAmounts().Any(x => x.Value > 0))
            {
                return next;
            }
            return this;
        }
        private bool GatheringFilter(TileMapMember member)
        {
            var gatherable = member.GetComponent<IGatherable>();
            if (gatherable == null || !gatherable.CanGather())
            {
                return false;
            }
            return !targetElement.HasValue || targetElement.Value == gatherable.GatherableType;
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