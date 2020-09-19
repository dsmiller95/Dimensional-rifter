using Assets.WorldObjects.Members.InteractionInterfaces;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNode
{
    /// <summary>
    /// Harvest the target object. for berries this means taking some amount of time and 
    ///     taking the berry out of the bush, and dropping it on the ground
    /// </summary>
    public class Harvest : Leaf
    {
        private string targetObjectInBlackboard;

        public Harvest(string targetObjectInBlackboard)
        {
            this.targetObjectInBlackboard = targetObjectInBlackboard;
        }

        protected override NodeStatus OnEvaluate(Blackboard blackboard)
        {
            if (blackboard.TryGetValueOfType(targetObjectInBlackboard, out GameObject targetObject))
            {
                var harvestable = targetObject?.GetComponent<IHarvestable>();
                if (harvestable == null) return NodeStatus.FAILURE;

                var harvestSuccess = harvestable.DoHarvest();
                if (harvestSuccess)
                {
                    return NodeStatus.SUCCESS;
                }
            }
            return NodeStatus.FAILURE;
        }

        public override void Reset(Blackboard blackboard)
        {
        }
    }
}
