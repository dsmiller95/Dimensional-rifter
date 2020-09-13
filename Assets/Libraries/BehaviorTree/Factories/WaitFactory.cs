using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [FactoryGraphNode("Leaf/Wait", "Wait", 0)]
    public class WaitFactory : LeafFactory
    {
        public float waitTime;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Wait(waitTime);
        }
    }
}
