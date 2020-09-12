using Assets.Behaviors.Scripts.UtilityStates;
using Assets.WorldObjects;

namespace Assets.Behaviors.Scripts.Tasks
{
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
