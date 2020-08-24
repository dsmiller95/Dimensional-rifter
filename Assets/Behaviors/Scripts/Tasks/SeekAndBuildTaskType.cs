using Assets.Behaviors.Scripts.FunctionalStates;
using Assets.WorldObjects.Members.Building;
using Assets.WorldObjects.Members.Food;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "SeekAndBuild", menuName = "Tasks/SeekAndBuild", order = 10)]
    public class SeekAndBuildTaskType : TaskType
    {
        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {
            if (sourceMember is TileMapNavigationMember navigation)
            {
                if (!navigation.AreAnyOfTypeReachable(GatheringFilter) || !navigation.AreAnyOfTypeReachable(BuildingDeliveryFilter))
                {
                    return null;
                }
                var resultState = new Gathering();
                resultState
                    .ContinueWith(new Storing(BuildingDeliveryFilter))
                    .ContinueWith(new Building())
                    .ContinueWith(returnToState);
                return resultState;
            }
            throw new System.Exception("Gathering requres a navigation member");
        }

        private bool GatheringFilter(TileMapMember member)
        {
            return member.GetComponent<Food>() != null;
        }
        private bool BuildingDeliveryFilter(TileMapMember member)
        {
            return member.GetComponent<Buildable>() != null;
        }
    }
}
