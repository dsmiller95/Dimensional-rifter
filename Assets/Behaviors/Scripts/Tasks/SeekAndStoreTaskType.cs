using Assets.Behaviors.Scripts.FunctionalStates;
using Assets.WorldObjects.Members.Food;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "SeekAndStore", menuName = "Tasks/SeekAndStore", order = 10)]
    public class SeekAndStoreTaskType : TaskType
    {
        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {
            if (sourceMember is TileMapNavigationMember navigation)
            {
                if (!navigation.AreAnyOfTypeReachable(GatheringFilter) || !navigation.AreAnyOfTypeReachable(StoringFilter))
                {
                    return null;
                }
                var resultState = new Gathering();
                resultState
                    .ContinueWith(new Storing())
                    .ContinueWith(returnToState);
                return resultState;
            }
            throw new System.Exception("Gathering requres a navigation member");
        }

        private bool GatheringFilter(TileMapMember member)
        {
            return member.GetComponent<Food>() != null;
        }
        private bool StoringFilter(TileMapMember member)
        {
            return member.memberType.recieveStorage == true;
        }
    }
}
