using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "Selector", menuName = "Behaviors/Control/Selector", order = 10)]
    [FactoryGraphNode("Composite/Selector", "Selector", -1)]
    public class SelectorFactory : CompositeFactory
    {
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new Selector(
                children.Select(child => child.CreateNode(target))
                );
        }
    }
}
