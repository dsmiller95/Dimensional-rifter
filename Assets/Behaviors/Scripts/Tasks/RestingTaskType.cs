using Assets.Behaviors.Scripts.UtilityStates;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    [CreateAssetMenu(fileName = "Rest", menuName = "Tasks/Rest", order = 11)]
    public class RestingTaskType : TaskType
    {
        public float restTime = .5f;
        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {
            var delay = new Delay<TileMapMember>(restTime);
            delay.ContinueWith(returnToState);

            return delay;
        }
    }
}
