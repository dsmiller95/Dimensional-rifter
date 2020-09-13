using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    public abstract class CompositeFactory : NodeFactory
    {
        public override int GetValidChildCount()
        {
            return -1;
        }
    }
}
