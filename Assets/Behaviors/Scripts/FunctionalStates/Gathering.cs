using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Food;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Gathering : ContinueableState<TileMapMember>
    {
        private Resource? targetElement;
        private ISet<ItemSourceType> validItemSources;

        public Gathering(ISet<ItemSourceType> validItemSources)
        {
            this.validItemSources = validItemSources;
        }

        public Gathering(Resource target, ISet<ItemSourceType> validItemSources) : this(validItemSources)
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

                var suppliers = seekResult.reached.GetComponents<ItemSource>();
                var myInventory = myInv.inventory;
                foreach (var supplier in suppliers)
                {
                    if (targetElement.HasValue)
                    {
                        supplier.GatherInto(myInventory, targetElement.Value);
                    }
                    else
                    {
                        supplier.GatherInto(myInventory);
                    }
                }

                // todo: no more gathering!
                seekResult.reached.GetComponent<IGatherable>()?.OnGathered();
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