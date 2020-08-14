using Assets.WorldObjects;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.HungryStates
{
    public class Foraging : GenericStateHandler<Hungry>
    {
        public Foraging()
        {
        }
        public GenericStateHandler<Hungry> HandleState(Hungry data)
        {
            var tileMember = data.GetComponent<TileMapNavigationMember>();
            if (tileMember.SeekClosestOfType(member => member.gameObject.GetComponent<Food>() != null))
            {
                var foundFood = tileMember.currentTarget.GetComponent<Food>();
                var sourceInv = data.GetComponent<ResourceInventory>();

                Object.Destroy(foundFood.gameObject);
                sourceInv.inventory.Add(Resource.FOOD, 1f);

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