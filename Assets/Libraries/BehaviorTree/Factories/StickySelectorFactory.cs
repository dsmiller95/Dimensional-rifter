﻿using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "StickySelector", menuName = "Behaviors/Control/StickySelector", order = 10)]
    public class StickySelectorFactory : NodeFactory
    {
        public NodeFactory[] children;

        protected override Node OnCreateNode(GameObject target)
        {
            return new Selector(
                children.Select(child => new CacheFirstResolution(
                        child.CreateNode(target)
                    ))
                );
        }
    }
}