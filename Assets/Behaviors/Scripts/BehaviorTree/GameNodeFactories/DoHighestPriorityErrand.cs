using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using Assets.UI.Priorities;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [FactoryGraphNode("Leaf/DoHighestPriorityErrand", "DoHighestPriorityErrand", 0)]
    public class DoHighestPriorityErrand : LeafFactory
    {
        public string errandPathInBlackboard;
        public PrioritySetToErrandConfiguration prioritySetToErrands;

        public ErrandBoard errandBoard;


        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Sequence(
                new ClaimHighestPriorityErrand(
                    target,
                    errandBoard,
                    prioritySetToErrands,
                    errandPathInBlackboard),
                new ExecuteErrand(errandPathInBlackboard)
            );
        }
    }
}
