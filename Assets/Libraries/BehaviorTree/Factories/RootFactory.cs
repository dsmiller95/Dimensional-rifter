using BehaviorTree.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "Root", menuName = "Behaviors/Control/Root", order = 10)]
    public class RootFactory : NodeFactory
    {
        public NodeFactory child;

        public override void SetChildFactories(IEnumerable<NodeFactory> children)
        {
            child = children.FirstOrDefault();
        }

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
#if UNITY_EDITOR
            FACTORY_RANDOM = new System.Random(GetInstanceID());
#endif
            var result = new Root(child.CreateNode(target));
#if UNITY_EDITOR
            FACTORY_RANDOM = null;
#endif
            return result;
        }
    }
}
