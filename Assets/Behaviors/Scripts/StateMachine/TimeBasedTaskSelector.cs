using Assets.Behaviors.Scripts.Tasks;
using Assets.WorldObjects;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts
{
    [Serializable]
    public struct TimezoneBasedTasks
    {
        public Timezone[] validTimezones;
        public TaskType[] prioritizedTasks;
    }

    //[CreateAssetMenu(fileName = "TimeBasedTaskSelector", menuName = "Tasks/TimeBasedSelector", order = 1)]
    public class TimeBasedTaskSelector : ScriptableObject, IGenericStateHandler<TileMapMember>
    {
        public GameTime timeProvider;
        public TimezoneBasedTasks[] tasksByTimeZone;

        public IGenericStateHandler<TileMapMember> HandleState(TileMapMember data)
        {
            var timezone = timeProvider.GetTimezone();
            var tasksByPriority = tasksByTimeZone.First(taskSet => taskSet.validTimezones.Contains(timezone));
            foreach (var task in tasksByPriority.prioritizedTasks)
            {
                var generatedState = task.TryGetEntryState(data, this);
                if (generatedState != null)
                {
                    return generatedState;
                }
            }
            Debug.LogWarning($"No valid tasks in timezone {Enum.GetName(typeof(Timezone), timezone)}. Repeating state.");
            return this;
        }

        public void TransitionIntoState(TileMapMember data)
        {
        }

        public void TransitionOutOfState(TileMapMember data)
        {
        }
        public override string ToString()
        {
            return $"Thinking 🤔";
        }
    }
}
