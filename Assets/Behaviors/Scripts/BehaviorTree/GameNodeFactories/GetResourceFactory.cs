using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using BehaviorTree.Factories;
using BehaviorTree.Nodes;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.BehaviorTree.GameNodeFactories
{

    [CreateAssetMenu(fileName = "GetResource", menuName = "Behaviors/Actions/GetResource", order = 10)]
    public class GetResourceFactory : NodeFactory
    {
        public Resource resourceType;
        public ItemSourceType[] validItemSources;
        public GenericSelector<IInventory<Resource>> selfInventoryToUse;

        public override Node CreateNode(GameObject target)
        {
            return GatherOfTypeFactory.GatherResourceOfType(
                target,
                validItemSources,
                selfInventoryToUse,
                resourceType);
        }
    }
}
