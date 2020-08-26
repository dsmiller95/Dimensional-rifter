using Assets.Behaviors.Scripts.Utility_states;
using Assets.WorldObjects;
using Assets.WorldObjects.Members.Food;
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

            var seekResult = tileMember.SeekClosestOfType(member => member.gameObject.GetComponent<Food>() != null);
            if (seekResult.status == NavigationStatus.ARRIVED)
            {
                var foundFood = seekResult.reached.GetComponent<Food>();

                Object.Destroy(foundFood.gameObject);
                myInv.inventory.Add(Resource.FOOD, 1f).Execute();

            }
            if (myInv.inventory.Get(Resource.FOOD) > 0)
            {
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
            return $"Gathering";
        }
    }
}