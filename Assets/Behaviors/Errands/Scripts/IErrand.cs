using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts
{
    public interface IErrand
    {
        ErrandType ErrandType { get; }
        /// <summary>
        /// method to check if the errand is still open. Should be pretty quick, if any pathing has to happen
        ///     do that on a different scedule
        /// </summary>
        /// <returns>true if the errand is still valid, otherwise false</returns>
        // bool CheckValid();

        /// <summary>
        /// Execute this errand.
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        NodeStatus Execute(Blackboard blackboard);

        void ClaimedBy(GameObject claimer);
    }
}
