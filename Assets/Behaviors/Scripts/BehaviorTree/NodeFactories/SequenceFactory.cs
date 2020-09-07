using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using System.Linq;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.NodeFactories
{
    [CreateAssetMenu(fileName = "Sequence", menuName = "Behaviors/Control/Sequence", order = 10)]
    public class SequenceFactory : NodeFactory
    {
        public NodeFactory[] children;

        public override Node CreateNode(GameObject target)
        {
            return new Sequence(
                children.Select(child => child.CreateNode(target))
                );
        }
    }
}
