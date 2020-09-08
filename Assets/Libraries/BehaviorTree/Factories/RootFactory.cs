using BehaviorTree.Nodes;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "Root", menuName = "Behaviors/Control/Root", order = 10)]
    public class RootFactory : NodeFactory
    {
        public NodeFactory child;

        protected override Node OnCreateNode(GameObject target)
        {
            return new Root(child.CreateNode(target));
        }
    }
}
