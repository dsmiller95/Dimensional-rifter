using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Factories;
using BehaviorTree.Factories.FactoryGraph;
using BehaviorTree.Nodes;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{

    [CreateAssetMenu(fileName = "GetResource", menuName = "Behaviors/Actions/GetResource", order = 10)]
    [FactoryGraphNode("Leaf/GetResource", "GetResource", 0)]
    [System.Obsolete("Use Entities")]
    public class GetResourceFactory : LeafFactory
    {
        public Resource resourceType;
        public ItemSourceType[] validItemSources;

        protected override BehaviorNode OnCreateNode(GameObject target)
        {
            return GatherOfTypeFactory.GatherResourceOfType(
                target,
                validItemSources,
                resourceType);
        }
    }
}
