using UnityEngine;

namespace Assets.Behaviors.Tasks
{
    [CreateAssetMenu(fileName = "StoreTask", menuName = "Tasks/Store", order = 1)]
    public class SeekAndStoreTaskType : TaskType
    {
        public override IGenericStateHandler<TileMapMember> TryGetEntryState(TileMapMember sourceMember, IGenericStateHandler<TileMapMember> returnToState)
        {

            //TODO : interact with the tilemapnavigationmember to get a possible path. and use that to construct the new state object
            // or if no path is possible then exit early
            throw new System.NotImplementedException();
        }
    }
}
