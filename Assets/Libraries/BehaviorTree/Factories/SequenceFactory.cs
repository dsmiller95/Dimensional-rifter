using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
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
