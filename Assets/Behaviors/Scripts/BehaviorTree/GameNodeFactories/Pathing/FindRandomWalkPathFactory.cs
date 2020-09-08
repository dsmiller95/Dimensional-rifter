﻿using Assets.Behaviors.Scripts.BehaviorTree.GameNode;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{
    [CreateAssetMenu(fileName = "FindRandomWalkPath", menuName = "Behaviors/Actions/FindRandomWalkPath", order = 10)]
    public class FindRandomWalkPathFactory : NodeFactory
    {
        public int randomWalkLength;
        public string blackboardPathProperty;
        protected override Node OnCreateNode(GameObject target)
        {
            return new SetRandomWalkTarget(
                target,
                blackboardPathProperty,
                randomWalkLength);
        }
    }
}