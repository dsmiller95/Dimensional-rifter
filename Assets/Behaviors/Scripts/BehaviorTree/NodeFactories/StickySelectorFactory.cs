using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Decorator;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.NodeFactories
{
    [CreateAssetMenu(fileName = "StickySelector", menuName = "Behaviors/Control/StickySelector", order = 10)]
    public class StickySelectorFactory : NodeFactory
    {
        public NodeFactory[] children;

        public override Node CreateNode(GameObject target)
        {
            return new Selector(
                children.Select(child => new CacheFirstResolution(
                        child.CreateNode(target)
                    ))
                );
        }
    }
}
