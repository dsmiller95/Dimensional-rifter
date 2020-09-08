using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "Selector", menuName = "Behaviors/Control/Selector", order = 10)]
    public class SelectorFactory : NodeFactory
    {
        public NodeFactory[] children;

        protected override Node OnCreateNode(GameObject target)
        {
            return new Selector(
                children.Select(child => child.CreateNode(target))
                );
        }
    }
}
