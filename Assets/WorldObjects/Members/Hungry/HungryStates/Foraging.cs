using Assets.Behaviors;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.WorldObjects.Members.Hungry.HungryStates
{
    public class Foraging : GenericStateHandler<Hungry>
    {
        public Foraging()
        {
        }
        public GenericStateHandler<Hungry> HandleState(Hungry data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            if (tileMember.SeekClosestOfType(member => member.gameObject.GetComponent<Food.Food>() != null))
            {
                var foundFood = tileMember.currentTarget.GetComponent<Food.Food>();
                var sourceInv = data.GetComponent<ResourceInventory>();

                Object.Destroy(foundFood.gameObject);
                sourceInv.inventory.Add(Resource.FOOD, 1f).Execute();

                return new Storing();
            }
            return this;
        }

        public void TransitionIntoState(Hungry data)
        {
        }
        public void TransitionOutOfState(Hungry data)
        {
        }
    }
}