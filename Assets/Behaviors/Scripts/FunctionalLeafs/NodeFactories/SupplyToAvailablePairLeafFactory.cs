using Assets.Behaviors.Scripts.BehaviorTree.NodeFactories;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes;
using Assets.Behaviors.Scripts.BehaviorTree.Nodes.Composite;
using Assets.Behaviors.Scripts.FunctionalLeafs.DataGrabbers;
using Assets.Behaviors.Scripts.FunctionalLeafs.StaticFactories;
using Assets.Scripts.Core;
using Assets.WorldObjects;
using Assets.WorldObjects.Inventories;
using TradeModeling.Inventories;
using UnityEngine;

namespace Assets.Behaviors.Scripts.FunctionalLeafs.NodeFactories
{
    [CreateAssetMenu(fileName = "SupplyToAvailablePair", menuName = "Behaviors/Actions/SupplyToAvailablePair", order = 10)]
    public class SupplyToAvailablePairLeafFactory : NodeFactory
    {
        public ItemSourceType[] validItemSources;
        public SuppliableType validSupplyTarget;
        public GenericSelector<IInventory<Resource>> selfInventoryToUse;

        public override Node CreateNode(GameObject target)
        {
            // TODO: check if I can supply from my current inventory, if I have anything?
            return
            new Sequence(
                new EnsureAvailableResourceSupplyPair(
                    target,
                    validItemSources,
                    validSupplyTarget,
                    "supplyType"
                ),
                GatherOfTypeFactory.GatherResourceOfType(
                    target,
                    validItemSources,
                    selfInventoryToUse,
                    "supplyType"
                ),
                SupplyToSupplyTypeFactory.SupplyAnyResourceFromSelfToSuppliable(
                    target,
                    validSupplyTarget,
                    selfInventoryToUse
                )
            );
        }
    }
}
