using Assets.Behaviors.Scripts.FunctionalStates;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "Build", menuName = "Tasks/Build", order = 10)]
    public class BuildTaskType : TaskType
    {
        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {
            if (sourceMember is TileMapNavigationMember navigation)
            {
                var buildingState = new Building();
                if (!navigation.AreAnyOfTypeReachable(buildingState.BuildingFilter))
                {
                    return null;
                }
                buildingState.ContinueWith(returnToState);
                return buildingState;
            }
            throw new System.Exception("Gathering requres a navigation member");
        }
    }
}
