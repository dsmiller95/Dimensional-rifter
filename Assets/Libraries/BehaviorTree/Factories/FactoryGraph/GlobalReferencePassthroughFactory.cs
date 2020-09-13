using BehaviorTree.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree.Factories.FactoryGraph
{
    /// <summary>
    /// factory which acts as a leaf in the behavior graph, used to reference another top-level
    ///     asset. probably another behavior graph factory
    /// </summary>
    [FactoryGraphNode("Decorator/GlobalReference", "GlobalReference", 0)]
    public class GlobalReferencePassthroughFactory : NodeFactory
    {
        public NodeFactory externalFactoryReference;

        public override void SetChildFactories(IEnumerable<NodeFactory> children)
        {
        }

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return externalFactoryReference.CreateNode(target);
        }
    }
}
