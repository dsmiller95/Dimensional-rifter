using Assets.Behaviors.Scripts;
using UnityEngine;

namespace Assets.WorldObjects.Members
{
    public class TaskExecutor : MonoBehaviour, IInterestingInfo
    {
        public TimeBasedTaskSelector taskSelector;

        private TileMapMember myMember;
        private StateMachine<TileMapMember> stateMachine;

        private void Awake()
        {
            myMember = GetComponent<TileMapMember>();
            stateMachine = new StateMachine<TileMapMember>(taskSelector);
        }

        private void Update()
        {
            stateMachine.update(myMember);
        }
        public string GetCurrentInfo()
        {
            return $"Task: {stateMachine.CurrentState}\n";
        }
    }
}
