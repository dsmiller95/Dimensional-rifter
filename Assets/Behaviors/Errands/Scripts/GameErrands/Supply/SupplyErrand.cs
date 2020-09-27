using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Errands.Scripts.GameErrands
{
    public class SupplyErrand : IErrand
    {
        private ItemSourceType itemSource;
        private SuppliableType supplyTarget;
        private SupplyErrandType errandType;
        public ErrandType ErrandType => errandType;

        public SupplyErrand(SupplyErrandType errandType)
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
