using Assets.WorldObjects;
using UnityEngine;

namespace Assets.Behaviors.Scripts.Tasks
{
    public abstract class TaskType : ScriptableObject
    {
        /// <summary>
        /// Get a state which will execute this task and return to <paramref name="returnToState"/> when the task is either complete or impossible.
        ///     Will return a null object if this task can't be executed because of a lack of compatible reachable objects in the world
        /// </summary>
        /// <param name="returnToState">The state to return to when the task completes</param>
        /// <returns>the new state</returns>
        public abstract IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState);
    }
}
