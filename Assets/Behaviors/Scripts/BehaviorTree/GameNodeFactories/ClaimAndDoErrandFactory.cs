using Assets.Behaviors.Errands.Scripts;
using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [FactoryGraphNode("Leaf/ClaimAndDoErrand", "ClaimAndDoErrand", 0)]
    public class ClaimAndDoErrandFactory : LeafFactory
    {
        public string tmpErrandPathInBlackboard;
        public ErrandBoard errandBoard;
        public ErrandType errandType;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Sequence(
                new ClaimErrandOfType(
                    target,
                    errandBoard,
                    errandType,
                    tmpErrandPathInBlackboard),
                new ExecuteErrand(tmpErrandPathInBlackboard)
            );
        }
    }
}
