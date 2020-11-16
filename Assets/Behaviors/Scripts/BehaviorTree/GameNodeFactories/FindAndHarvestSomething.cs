using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using System;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [FactoryGraphNode("Leaf/FindAndHarvest", "FindAndHarvest", 0)]
    [Obsolete("Use Entities")]
    public class FindAndHarvestSomething : LeafFactory
    {
        public string harvestTargetInBlackboard = "Target";
        public string harvestPathInBlackboard = "Path";

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Sequence(
                    new FindHarvestableTarget(target, harvestPathInBlackboard),
                    new NavigateToTarget(target, harvestPathInBlackboard, harvestTargetInBlackboard),
                    new Harvest(harvestTargetInBlackboard)
                );
        }
    }
}
