using Assets.Behaviors.Scripts.FunctionalStates;
using Assets.Behaviors.Scripts.UtilityStates;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using Assets.WorldObjects.Members.Hungry;
using System.Collections.Generic;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "SeekAndEat", menuName = "Tasks/SeekAndEat", order = 10)]
    public class SeekAndEatTaskType : TaskType
    {
        [Tooltip("If current hunger is below this threshold, this will not produce a task")]
        public float hungerThreshold;
        [Tooltip("How much time is spent eating after retrieving the food")]
        public float eatingTime = 1f;

        public ItemSourceType[] validItemSources;
        private ISet<ItemSourceType> _validItems;
        private ISet<ItemSourceType> ValidItemSourceTypes
        {
            get
            {
                if (_validItems == null)
                {
                    _validItems = new HashSet<ItemSourceType>(validItemSources);
                }
                return _validItems;
            }
        }
        public GenericSelector<IInventory<Resource>> selfInventoryToUse;

        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {
            if (sourceMember is TileMapNavigationMember navigation)
            {
                var hungery = sourceMember.GetComponent<Hungry>();
                if (hungery.currentHunger < hungerThreshold)
                {
                    return null;
                }
                var foodGatherState = new Gathering(selfInventoryToUse, ValidItemSourceTypes, Resource.FOOD);
                if (!navigation.AreAnyOfTypeReachable(foodGatherState.GatheringFilter))
                {
                    return null;
                }

                foodGatherState
                    .ContinueWith(new Eating(selfInventoryToUse))
                    .ContinueWith(new Delay<TileMapMember>(eatingTime))
                    .ContinueWith(returnToState);

                return foodGatherState;
            }
            //TODO : interact with the tilemapnavigationmember to get a possible path. and use that to construct the new state object
            // or if no path is possible then exit early
            throw new System.Exception("Seeking to eat requres a navigation member");
        }
    }
}
