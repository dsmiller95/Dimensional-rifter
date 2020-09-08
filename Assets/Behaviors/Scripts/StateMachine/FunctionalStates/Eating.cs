using Assets.Behaviors.Scripts.Utility_states;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Members.Hungry;
using TradeModeling.Inventories;

namespace Assets.Behaviors.Scripts.FunctionalStates
{
    public class Eating : ContinueableState<TileMapMember>
    {
        private GenericSelector<IInventory<Resource>> inventoryToEatFrom;
        public Eating(GenericSelector<IInventory<Resource>> inventoryToEatFrom)
        {
            this.inventoryToEatFrom = inventoryToEatFrom;
        }

        public override IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var hungry = data.GetComponent<Hungry>();
            var stateSource = data.GetComponent<VariableInstantiator>();
            var selfInv = inventoryToEatFrom.GetCurrentValue(stateSource);

            var consumption = selfInv.Consume(Resource.FOOD, hungry.currentCalories);
            hungry.currentCalories -= consumption.info;
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