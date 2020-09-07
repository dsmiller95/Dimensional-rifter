using Assets.Behaviors.Scripts.BehaviorTree.NodeFactories;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.NodeFactories
{
    [CreateAssetMenu(fileName = "Root", menuName = "Behaviors/Control/Root", order = 10)]
    public class RootFactory : NodeFactory
    {
        public NodeFactory child;

        public override Node CreateNode(GameObject target)
        {
            return new Root(child.CreateNode(target));
        }
    }
}
