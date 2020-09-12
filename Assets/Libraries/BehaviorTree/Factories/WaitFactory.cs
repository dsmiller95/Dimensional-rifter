﻿using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    //[CreateAssetMenu(fileName = "Selector", menuName = "Behaviors/Control/Selector", order = 10)]
    public class WaitFactory: NodeFactory
    {
        public float waitTime;

        protected override Node OnCreateNode(GameObject target)
        {
            return new Wait(waitTime);
        }
    }
}