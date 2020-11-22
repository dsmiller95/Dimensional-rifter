using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "Sequence", menuName = "Behaviors/Control/Sequence", order = 10)]
    [FactoryGraphNode("Composite/Sequence", "Sequence", -1)]
    public class SequenceFactory : CompositeFactory
    {
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Sequence(
                children
                    .Where(x => x != null)
                    .Select(child => child.CreateNode(target))
                );
        }
    }
}
