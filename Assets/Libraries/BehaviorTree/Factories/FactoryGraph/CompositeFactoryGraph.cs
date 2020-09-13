using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories.FactoryGraph
{
    [CreateAssetMenu(fileName = "FactoryGraph", menuName = "Behaviors/FactoryGraph", order = 1)]
    public class CompositeFactoryGraph : NodeFactory
    {
        public FactoryNodeSavedNode savedNodes;

        public NodeFactory entryFactory;

        public override int GetValidChildCount()
        {
            return 0;
        }

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return null;
        }
    }
}
