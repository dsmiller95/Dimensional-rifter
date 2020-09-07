using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.NodeFactories
{
    [CreateAssetMenu(fileName = "Selector", menuName = "Behaviors/Control/Selector", order = 10)]
    public class SelectorFactory : NodeFactory
    {
        public NodeFactory[] children;

        public override Node CreateNode(GameObject target)
        {
            return new Selector(
                children.Select(child => child.CreateNode(target))
                );
        }
    }
}
