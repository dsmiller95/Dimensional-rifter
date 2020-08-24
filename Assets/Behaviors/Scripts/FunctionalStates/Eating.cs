using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using Assets.WorldObjects.Members.Hungry;
using TradeModeling.Inventories;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Eating : ContinueableState<TileMapMember>
    {
        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var hungry = data.GetComponent<Hungry>();
            var selfInv = data.GetComponent<ResourceInventory>();

            var consumption = selfInv.inventory.Consume(Resource.FOOD, hungry.currentHunger);
            hungry.currentHunger -= consumption.info;
            consumption.Execute();

            return next;
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
            return $"Eating";
        }
    }
}