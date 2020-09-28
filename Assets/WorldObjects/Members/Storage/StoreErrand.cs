using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.WorldObjects.Members.Storage
{
    public class StoreErrand : IErrand
    {
        private StoreErrandType errandType;
        public ErrandType ErrandType => errandType;

        public StoreErrand(StoreErrandType errandType)
        {
            this.errandType = errandType;
        }

        private BehaviorNode ErrandBehaviorTreeRoot;

        public void ClaimedBy(GameObject claimer)
        {
            throw new NotImplementedException();
        }

        public NodeStatus Execute(Blackboard blackboard)
        {
            return ErrandBehaviorTreeRoot?.Evaluate(blackboard) ?? NodeStatus.FAILURE;
        }

        public void OnReset()
        {
            throw new NotImplementedException();
        }
    }
}
