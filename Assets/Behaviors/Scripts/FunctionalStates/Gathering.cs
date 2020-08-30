using Assets.Behaviors.Scripts.Utility_states;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Food;
using System.Collections.Generic;
using System.Linq;
using TradeModeling.Inventories;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Gathering : ContinueableState<TileMapMember>
    {
        private Resource? targetElement;
        private ISet<ItemSourceType> validItemSources;
        private GenericSelector<IInventory<Resource>> inventoryToGatherInto;

        public Gathering(GenericSelector<IInventory<Resource>> inventoryToGatherInto, ISet<ItemSourceType> validItemSources)
        {
            this.validItemSources = validItemSources;
            this.inventoryToGatherInto = inventoryToGatherInto;
        }

        public Gathering(GenericSelector<IInventory<Resource>> inventoryToGatherInto, ISet<ItemSourceType> validItemSources, Resource target) : this(inventoryToGatherInto, validItemSources)
        {
            targetElement = target;
        }

        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var stateSource = data.GetComponent<VariableInstantiator>();
            var selfInv = this.inventoryToGatherInto.GetCurrentValue(stateSource);

            var seekResult = tileMember.SeekClosestOfType(GatheringFilter);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {

                var suppliers = seekResult.reached.GetComponents<ItemSource>();
                foreach (var supplier in suppliers)
                {
                    if (targetElement.HasValue)
                    {
                        supplier.GatherInto(selfInv, targetElement.Value);
                    }
                    else
                    {
                        supplier.GatherInto(selfInv);
                    }
                }

                // todo: no more gathering!
                seekResult.reached.GetComponent<IGatherable>()?.OnGathered();
                return next;
            }
            // TODO:
            //  This is to "recover" from a save state where I have something in my inventory
            //  this really should be done at decision-time, not on every loop
            if (selfInv.GetCurrentResourceAmounts().Any(x => x.Value > 0))
            {
                return next;
            }
            return this;
        }
        public bool GatheringFilter(TileMapMember member)
        {
            if (!targetElement.HasValue)
            {
                return true;
            }
            var suppliers = member.GetComponents<ItemSource>()
                .Where(x => validItemSources.Contains(x.SourceType) && x.HasResource(targetElement.Value));
            return suppliers.Any();
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