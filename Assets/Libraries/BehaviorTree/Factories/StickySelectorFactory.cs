using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using System.Linq;
using UnityEngine;

namespace BehaviorTree.Factories
{
    [CreateAssetMenu(fileName = "StickySelector", menuName = "Behaviors/Control/StickySelector", order = 10)]
    [FactoryGraphNode("Composite/StickySelector", "StickySelector", -1)]
    public class StickySelectorFactory : CompositeFactory
    {
        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return new StickySelector(
                children.Select(child => child.CreateNode(target))
                );
        }
    }
}
