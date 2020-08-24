using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using TradeModeling.Inventories;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Storing : ContinueableState<TileMapMember>
    {
        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            var seekResult = tileMember.SeekClosestOfType(member => member.memberType.recieveStorage == true);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var storageInv = seekResult.reached.GetComponent<ResourceInventory>();
                var sourceInv = data.GetComponent<ResourceInventory>();

                sourceInv.inventory.DrainAllInto(storageInv.inventory, System.Enum.GetValues(typeof(Resource)) as Resource[]);

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
            return $"Storing";
        }
    }
}
